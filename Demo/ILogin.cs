using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Demo
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
        /// <summary>
        /// 测试命令
        /// </summary>
        ICommand Test { get; }
    }
}

namespace Demo.Impl
{
    public class ILogin : QM_ViewModelBase, Demo.ILogin
    {
        private String _UserName;
        public string UserName
        {
            get { return _UserName; }
            set
            {
                _UserName = value;
                ((DelegateCommand)Login).RaiseCanExecuteChanged();
            }
        }

        public System.Windows.Input.ICommand Login { get; private set; }
        public System.Windows.Input.ICommand ChangeTheme { get; private set; }
        public System.Windows.Input.ICommand ChangeLanguage { get; private set; }
        public System.Windows.Input.ICommand Test { get; private set; }

        public override void Init()
        {
            Login = new DelegateCommand()
            {
                ExecuteCommand = executeCommand_Login,
                CanExecuteCommand = canExecuteCommand_Login
            };
            ChangeTheme = new DelegateCommand() { ExecuteCommand = executeCommand_ChangeTheme };
            ChangeLanguage = new DelegateCommand() { ExecuteCommand = executeCommand_ChangeLanguage };
            Test = new DelegateCommand() { ExecuteCommand = executeCommand_Test };
        }

        /// <summary>
        /// 执行登录命令
        /// </summary>
        /// <param name="obj"></param>
        private void executeCommand_Login(Object obj)
        {
            Navigator.Navigate<IMain>();
        }

        /// <summary>
        /// 返回能否执行登录命令
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool canExecuteCommand_Login(Object obj)
        {
            return !String.IsNullOrWhiteSpace(UserName);
        }

        private void executeCommand_ChangeTheme(object argument)
        {
            Start.ViewManager.CurrentTheme = argument.ToString();
        }

        private void executeCommand_ChangeLanguage(object argument)
        {
            Start.ViewManager.CurrentLanguage = argument.ToString();
        }

        private void executeCommand_Test(object obj)
        {
            new Window()
            {
                Content = Start.ViewManager.GetView<Test.IView>()
            }.ShowDialog();
        }
    }
}