using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quick.MVVM.Localization
{
    /// <summary>
    /// 文本特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TextAttribute : Attribute
    {
        /// <summary>
        /// 值
        /// </summary>
        public String Value { get; set; }
        public TextAttribute(String value)
        {
            this.Value = value;
        }
    }
}
