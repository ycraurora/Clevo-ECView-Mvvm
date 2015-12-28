using Caliburn.Micro;
using ECView.ViewModels;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ECView.Frameworks
{
    public class AppBootstrapper : BootstrapperBase
    {
        [CanBeNull] private SimpleContainer _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<IShell, EcMainViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = _container?.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        [CanBeNull]
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container?.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container?.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }
    }
}