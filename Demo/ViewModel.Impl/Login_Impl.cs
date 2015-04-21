using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Demo.ViewModel.Impl
{
    public class Login_Impl : QM_ViewModelBase, ILogin
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

        public override void Init()
        {
            Login = new DelegateCommand()
            {
                ExecuteCommand = executeCommand_Login,
                CanExecuteCommand = canExecuteCommand_Login
            };
            ChangeTheme = new DelegateCommand() { ExecuteCommand = executeCommand_ChangeTheme };
            ChangeLanguage = new DelegateCommand() { ExecuteCommand = executeCommand_ChangeLanguage };
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
    }
}
