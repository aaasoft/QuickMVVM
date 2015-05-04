using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quick.MVVM.Localization
{
    /// <summary>
    /// 文本特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class TextAttribute : Attribute
    {
        /// <summary>
        /// 序号
        /// </summary>
        public Int32 Index { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public String Value { get; set; }
        public TextAttribute(Int32 index, String value)
        {
            this.Index = index;
            this.Value = value;
        }
    }
}
