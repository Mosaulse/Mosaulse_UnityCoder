/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// 解决方案项目条目
    /// </summary>
    internal class SolutionProjectEntry
    {
        public string ProjectFactoryGuid { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string ProjectGuid { get; set; }
        public string Metadata { get; set; }

        public bool IsSolutionFolderProjectFactory()
        {
            return ProjectFactoryGuid != null && ProjectFactoryGuid.Equals("2150E333-8FDC-42A3-9474-1A3956D46DE8", System.StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// 解决方案属性
    /// </summary>
    internal class SolutionProperties
    {
        public string Name { get; set; }
        public System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<string, string>> Entries { get; set; }
        public string Type { get; set; }
    }

    /// <summary>
    /// 解决方案类
    /// </summary>
    internal class Solution
    {
        public SolutionProjectEntry[] Projects { get; set; }
        public SolutionProperties[] Properties { get; set; }
    }
}

