using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Quick.MVVM.ViewModel
{
    public class ViewModelManager : IViewModelManager
    {
        private HashSet<Assembly> scanedAssemblyHashSet = new HashSet<Assembly>();
        private Dictionary<Type, Type> viewModelTypeViewModelImplTypeDict = new Dictionary<Type, Type>();

        public Action<IViewModel> AfterViewModelCreated { get; set; }

        public void RegisterViewModel<TViewModelType, TViewModelImplType>()
        {
            RegisterViewModel(typeof(TViewModelType), typeof(TViewModelImplType));
        }

        public TViewModelType CreateInstance<TViewModelType>()
            where TViewModelType : class,IViewModel
        {
            return CreateInstance<TViewModelType>(null);
        }

        public TViewModelType CreateInstance<TViewModelType>(Action<TViewModelType> initAction)
            where TViewModelType : class,IViewModel
        {
            TViewModelType viewModel = _CreateInstance(typeof(TViewModelType)) as TViewModelType;
            if (viewModel == null)
                return null;
            if (initAction != null)
                initAction(viewModel);
            viewModel.Init();
            viewModel.IsInited = true;
            return viewModel;
        }

        public IViewModel CreateInstance(Type viewModelType)
        {
            return CreateInstance(viewModelType, null);
        }

        public IViewModel CreateInstance(Type viewModelType, Action<IViewModel> initAction)
        {
            IViewModel viewModel = _CreateInstance(viewModelType);
            if (viewModel == null)
                return null;
            if (initAction != null)
                initAction(viewModel);
            viewModel.Init();
            viewModel.IsInited = true;
            return viewModel;
        }
        
        private IViewModel _CreateInstance(Type viewModelType)
        {
            if (viewModelType == null)
                return null;
            if (!viewModelType.IsInterface)
                throw new ApplicationException("ViewModel type must be an 'interface',not a 'class'.");

            //如果此程序集没有扫描过，则扫描此程序集中所有的类
            if (!scanedAssemblyHashSet.Contains(viewModelType.Assembly))
            {
                foreach (Type type in viewModelType.Assembly.GetTypes())
                    scanType(type);
            }

            if (!viewModelTypeViewModelImplTypeDict.ContainsKey(viewModelType))
                return null;
            Type viewModelImplType = viewModelTypeViewModelImplTypeDict[viewModelType];
            IViewModel viewModel = (IViewModel)Activator.CreateInstance(viewModelImplType);
            if (AfterViewModelCreated != null)
                AfterViewModelCreated.Invoke(viewModel);
            return viewModel;
        }

        /// <summary>
        /// 注册视图模型
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <param name="viewModelImplType"></param>
        public void RegisterViewModel(Type viewModelType, Type viewModelImplType)
        {
            viewModelTypeViewModelImplTypeDict[viewModelType] = viewModelImplType;
        }

        // 搜索视图模型
        private void scanType(Type type)
        {
            if (!typeof(IViewModel).IsAssignableFrom(type))
                return;

            if (!type.IsClass || type.IsAbstract)
                return;

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsClass
                    || interfaceType == typeof(IViewModel)
                    || !typeof(IViewModel).IsAssignableFrom(interfaceType))
                    continue;
                //注册视图模型
                RegisterViewModel(interfaceType, type);
            }
        }

        /// <summary>
        /// 得到视图模型对象的接口类型
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public Type GetViewModelInterfaceType(IViewModel viewModel)
        {
            Type viewModelImplType = viewModel.GetType();
            foreach (Type interfaceType in viewModelImplType.GetInterfaces())
            {
                if (interfaceType == typeof(IViewModel))
                    continue;
                if (typeof(IViewModel).IsAssignableFrom(interfaceType))
                    return interfaceType;
            }
            return null;
        }
    }
}
