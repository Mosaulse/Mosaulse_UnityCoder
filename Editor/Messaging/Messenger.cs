/*--------------------------------------------------------------------------------------------- *  Copyright (c) UnityCoder Team. All rights reserved. *  Licensed under the MIT License. See License.txt in the project root for license information. *--------------------------------------------------------------------------------------------*/
using System;
using System.Net;
using System.Net.Sockets;

namespace UnityCoder.Editor.Integration.Messaging
{
	internal class Messager : IDisposable
	{
		public event EventHandler<MessageEventArgs> ReceiveMessage;
		public event EventHandler<ExceptionEventArgs> MessagerException;

		private readonly UdpSocket _socket;
		private bool _disposed;

#if UNITY_EDITOR_WIN
		[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetHandleInformation(IntPtr hObject, HandleFlags dwMask, HandleFlags dwFlags);

		[Flags]
		private enum HandleFlags: uint
		{
			None = 0,
			Inherit = 1,
			ProtectFromClose = 2
		}
#endif

		protected Messager(int port)
		{
			_socket = new UdpSocket();
			_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
			_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

#if UNITY_EDITOR_WIN
			// Explicitely disable inheritance for our UDP socket handle 
			// We found that Unity is creating a fork when importing new assets that can clone our socket
			SetHandleInformation(_socket.Handle, HandleFlags.Inherit, HandleFlags.None);
#endif

			_socket.Bind(IPAddress.Any, port);

			BeginReceiveMessage();
		}

		private void BeginReceiveMessage()
		{
			var buffer = new byte[UdpSocket.BufferSize];
			var any = UdpSocket.Any();

			try
			{
			beginReceive:
				if (_disposed)
					return;

				var result = _socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref any, ReceiveMessageCallback, buffer);
				if (result.CompletedSynchronously)
					goto beginReceive;
			}
			catch (SocketException se)
			{
				MessagerException?.Invoke(this, new ExceptionEventArgs(se));

				BeginReceiveMessage();
			}
			catch (ObjectDisposedException)
			{
			}
		}

		private void ReceiveMessageCallback(IAsyncResult result)
		{
			try
			{
				var endPoint = UdpSocket.Any();

				if (_disposed)
					return;

				_socket.EndReceiveFrom(result, ref endPoint);

				var message = DeserializeMessage(UdpSocket.BufferFor(result));
				if (message != null)
				{
					message.Origin = (IPEndPoint)endPoint;

					// 简化实现，移除对TCP模式的支持
					ReceiveMessage?.Invoke(this, new MessageEventArgs(message));
				}
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			catch (Exception e)
			{
				RaiseMessagerException(e);
			}

			if (!result.CompletedSynchronously)
			BeginReceiveMessage();
	}

	private void RaiseMessagerException(Exception e)
		{
			MessagerException?.Invoke(this, new ExceptionEventArgs(e));
		}

		private static Message MessageFor(MessageType type, string value)
		{
			return new Message { Type = type, Value = value };
		}

		public void SendMessage(IPEndPoint target, MessageType type, string value = "")
		{
			var message = MessageFor(type, value);
			var buffer = SerializeMessage(message);

			try
			{
				if (_disposed)
					return;

				// 简化实现，移除对TCP模式的支持
				_socket.BeginSendTo(buffer, 0, Math.Min(buffer.Length, UdpSocket.BufferSize), SocketFlags.None, target, SendMessageCallback, null);
			}
			catch (SocketException se)
			{
				MessagerException?.Invoke(this, new ExceptionEventArgs(se));
			}
			catch (ObjectDisposedException)
			{
			}
		}

		private void SendMessageCallback(IAsyncResult result)
		{
			try
			{
				if (_disposed)
					return;

				_socket.EndSendTo(result);
			}
			catch (SocketException se)
			{
				MessagerException?.Invoke(this, new ExceptionEventArgs(se));
			}
			catch (ObjectDisposedException)
			{
			}
		}

		private static byte[] SerializeMessage(Message message)
		{
			// 简化实现，使用简单的序列化方法
			using (var stream = new System.IO.MemoryStream())
			using (var writer = new System.IO.BinaryWriter(stream))
			{
				writer.Write((int)message.Type);
				writer.Write(message.Value ?? string.Empty);
				return stream.ToArray();
			}
		}

		private static Message DeserializeMessage(byte[] buffer)
		{
			if (buffer.Length < 4)
				return null;

			// 简化实现，使用简单的反序列化方法
			using (var stream = new System.IO.MemoryStream(buffer))
			using (var reader = new System.IO.BinaryReader(stream))
			{
				var type = (MessageType)reader.ReadInt32();
				var value = reader.ReadString();
				return new Message { Type = type, Value = value };
			}
		}

		public static Messager BindTo(int port)
		{
			return new Messager(port);
		}

		public void Dispose()
		{
			try
			{
				_disposed = true;
				_socket.Close();
			}
			catch
			{
			}
		}
	}
}
