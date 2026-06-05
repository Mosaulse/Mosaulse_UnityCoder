/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// 异步操作包装类
    /// </summary>
    /// <typeparam name="T">结果类型</typeparam>
    internal class AsyncOperation<T>
    {
        private readonly Task<T> _task;
        private T _result;
        private bool _completed;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="operation">异步操作</param>
        private AsyncOperation(Func<T> operation)
        {
            _task = Task.Run(operation);
        }

        /// <summary>
        /// 运行异步操作
        /// </summary>
        /// <param name="operation">操作函数</param>
        /// <returns>异步操作对象</returns>
        public static AsyncOperation<T> Run(Func<T> operation)
        {
            return new AsyncOperation<T>(operation);
        }

        /// <summary>
        /// 运行异步操作（带错误处理）
        /// </summary>
        /// <param name="operation">操作函数</param>
        /// <param name="errorHandler">错误处理函数</param>
        /// <param name="completionHandler">完成处理函数</param>
        /// <returns>异步操作对象</returns>
        public static AsyncOperation<T> Run(Func<T> operation, Func<Exception, T> errorHandler, Action completionHandler = null)
        {
            return new AsyncOperation<T>(() =>
            {
                try
                {
                    var result = operation();
                    completionHandler?.Invoke();
                    return result;
                }
                catch (Exception ex)
                {
                    completionHandler?.Invoke();
                    return errorHandler(ex);
                }
            });
        }

        /// <summary>
        /// 获取操作结果
        /// </summary>
        public T Result
        {
            get
            {
                if (!_completed)
                {
                    _result = _task.Result;
                    _completed = true;
                }
                return _result;
            }
        }

        /// <summary>
        /// 检查操作是否完成
        /// </summary>
        public bool IsCompleted => _completed || _task.IsCompleted;
    }
}

