using Quick.MVVM.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Quick.MVVM.ViewModel
{
    public abstract class ViewModelBase : NotifyPropertyChangedModelBase, IViewModel
    {
        private Object _View;
        /// <summary>
        /// 此视图模型对应的视图对象
        /// </summary>
        public virtual Object View
        {
            get { return _View; }
            set
            {
                _View = value;
                OnViewLoaded();
            }
        }
        public virtual INavigator Navigator { get; set; }

        /// <summary>
        /// 初始化方法
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 当视图加载时
        /// </summary>
        public virtual void OnViewLoaded() { }
    }
}
