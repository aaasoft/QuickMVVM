using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Demo.ViewModel
{
    public interface ILogin : IViewModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        String UserName { get; set; }
        /// <summary>
        /// 登录命令(PasswordBox控件作为此命令的参数)
        /// </summary>
        ICommand Login { get; }
        /// <summary>
        /// 改变主题
        /// </summary>
        ICommand ChangeTheme { get; }
        /// <summary>
        /// 改变语言
        /// </summary>
        ICommand ChangeLanguage { get; }
    }
}
