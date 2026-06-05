/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// 版本对映射类，用于IDE版本到C#语言版本的映射
    /// </summary>
    internal class VersionPair
    {
        /// <summary>
        /// IDE版本
        /// </summary>
        public Version IdeVersion { get; }

        /// <summary>
        /// 对应的C#语言版本
        /// </summary>
        public Version LanguageVersion { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ideMajor">IDE主版本号</param>
        /// <param name="ideMinor">IDE次版本号</param>
        /// <param name="languageMajor">C#语言主版本号</param>
        /// <param name="languageMinor">C#语言次版本号</param>
        public VersionPair(int ideMajor, int ideMinor, int languageMajor, int languageMinor)
        {
            IdeVersion = new Version(ideMajor, ideMinor);
            LanguageVersion = new Version(languageMajor, languageMinor);
        }

        /// <summary>
        /// 构造函数（使用Version对象）
        /// </summary>
        /// <param name="ideVersion">IDE版本</param>
        /// <param name="languageVersion">C#语言版本</param>
        public VersionPair(Version ideVersion, Version languageVersion)
        {
            IdeVersion = ideVersion;
            LanguageVersion = languageVersion;
        }
    }
}

