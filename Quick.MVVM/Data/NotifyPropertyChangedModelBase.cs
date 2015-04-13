using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Quick.MVVM.Data
{
    /// <summary>
    /// 通知属性更改的模型基类
    /// </summary>
    public abstract class NotifyPropertyChangedModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性已改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性已改变事件
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
