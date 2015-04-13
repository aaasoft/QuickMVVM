using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quick.MVVM.View
{
    /// <summary>
    /// XAML解析过滤器接口
    /// </summary>
    public interface IXamlFilter
    {
        /// <summary>
        /// 解析之前
        /// </summary>
        /// <param name="xamlContent"></param>
        /// <returns></returns>
        String Before(String xamlContent);
        /// <summary>
        /// 解析之后
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        Object After(Object content);
    }
}
