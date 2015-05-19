using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Demo
{
    public interface IMain : IViewModel
    {
        
    }

    class _IMain : QM_ViewModelBase, IMain
    {
        public override void Init()
        {

        }
    }
}