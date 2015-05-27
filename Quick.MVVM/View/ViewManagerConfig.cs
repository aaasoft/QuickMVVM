using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quick.MVVM.View
{
    /// <summary>
    /// 视图管理器配置
    /// </summary>
    public class ViewManagerConfig
    {
        /// <summary>
        /// 主题目录
        /// </summary>
        public String ThemeFolder { get; set; }
        /// <summary>
        /// 程序集中的主题路径
        /// </summary>
        public String ThemePathInAssembly { get; set; }
        /// <summary>
        /// 语言目录
        /// </summary>
        public String LanguageFolder { get; set; }
        /// <summary>
        /// 程序集中的语言路径
        /// </summary>
        public String LanguagePathInAssembly { get; set; }
        /// <summary>
        /// 视图文件后缀
        /// </summary>
        public String ViewFileExtension { get; set; }
        /// <summary>
        /// 语言文件后缀
        /// </summary>
        public String LanguageFileExtension { get; set; }
    }
}
