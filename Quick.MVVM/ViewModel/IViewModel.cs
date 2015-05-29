using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Quick.MVVM.ViewModel
{
    public interface IViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 此视图模型对应的视图对象
        /// </summary>
        Object View { get; set; }
        /// <summary>
        /// 界面导航器
        /// </summary>
        INavigator Navigator { get; set; }
        /// <summary>
        /// 初始化方法
        /// </summary>
        void Init();
        /// <summary>
        /// 当视图加载时
        /// </summary>
        void OnViewLoaded();
    }
}
