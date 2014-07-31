using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using Autofac.Core;

namespace Caliburn.Micro.Autofac
{
    public abstract class CaliburnAutofacApplication : CaliburnApplication, IActivateComponent, ISessionEvents
    {
        protected IContainer Container;
        private readonly ContainerBuilder _builder;
        private AutofacFrameAdapter _frameAdapter;
        private Frame _rootFrame;
        readonly IDictionary<Type, ViewScope> _viewsToScope = new Dictionary<Type, ViewScope>();
        readonly object _framePlaceholder = new object();
        protected object NavigationContext { get; set; }

        public event Action<object> Activated = _ => { };
        public event EventHandler<NavigatingCancelEventArgs> Navigating;
        public event EventHandler<ViewModelDisposedEventArgs> ViewModelDisposed;
        public event EventHandler NewSession;
        protected ISharingService SharingService { get; private set; }

        protected CaliburnAutofacApplication()
        {
            _builder = new ContainerBuilder();
        }

        protected override void Configure()
        {
            _builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            _builder.Register(x => _frameAdapter).As<INavigationService>().SingleInstance();
            _builder.Register(x => Container).As<IContainer>().SingleInstance();
            _builder.RegisterInstance(this).AsSelf().As<IActivateComponent>().As<ISessionEvents>().SingleInstance();

            _builder.RegisterType<SharingService>().As<ISharingService>().SingleInstance();
            _builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();
            RegisterViewModels(x => typeof(INotifyPropertyChanged).IsAssignableFrom(x));
            _builder.RegisterAssemblyModules(AssemblySource.Instance.ToArray());

            HandleConfigure(_builder);
            Container = _builder.Build();

            ViewModelLocator.LocateForView = LocateForView;
            SharingService = Container.Resolve<ISharingService>();
            _rootFrame = CreateApplicationFrame();
        }

        public virtual void RegisterViewModels(Predicate<Type> isViewModel)
        {
            _builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(x => isViewModel(x))
                .AsSelf()
                .InstancePerDependency()
                .Named(x => x.FullName, typeof(INotifyPropertyChanged))
                .OnActivated(OnActivated);
        }

        protected virtual object LocateForView(object view)
        {
            if (view == null)
                return null;
            var element = view as FrameworkElement;
            if (element != null && element.DataContext != null && (element.DataContext as INotifyPropertyChanged != null))
                return element.DataContext;

            var viewType = view.GetType();
            if (!_viewsToScope.ContainsKey(viewType))
            {
                var scope = Container.BeginLifetimeScope(builder => builder.RegisterInstance(view)
                    .AsSelf()
                    .AsImplementedInterfaces());
                _viewsToScope.Add(viewType, new ViewScope(view, scope));
            }
            else
            {
                var current = _viewsToScope[viewType];
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterInstance(view).AsSelf().AsImplementedInterfaces();
                containerBuilder.Update(current.LifetimeScope.ComponentRegistry);
                _viewsToScope[viewType] = new ViewScope(view, current.LifetimeScope);
            }

            return ViewModelLocator.LocateForViewType(viewType);
        }

        protected override object GetInstance(Type service, string key)
        {
            var scope = _viewsToScope[CurrentSourcePageType];

            try
            {
                object instance;
                if (string.IsNullOrEmpty(key))
                {
                    if (scope.LifetimeScope.TryResolve(service, out instance))
                        return instance;
                }
                else
                {
                    //caliburn can ask for a Keyed service without providing the type,
                    //to fullfil this we must scan the actual component registry
                    if (service == null)
                    {
                        var unTyped = scope.LifetimeScope.ComponentRegistry.Registrations.Concat(Container.ComponentRegistry.Registrations).SelectMany(
                            x => x.Services.OfType<KeyedService>().Where(y => y.ServiceKey as string == key)).FirstOrDefault();

                        if (unTyped == null)
                            throw new DependencyResolutionException(string.Format("Unable to locate a service type for key {0}", key));

                        service = unTyped.ServiceType;
                    }

                    if (scope.LifetimeScope.TryResolveNamed(key, service, out instance))
                        return instance;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (Debugger.IsAttached) Debugger.Break();
                throw;
            }

            throw new Exception(string.Format("Could not locate any instances of service {0}.", service.Name));
        }

        private Type CurrentSourcePageType
        {
            get { return _navigatingTo ?? _rootFrame.CurrentSourcePageType; }
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var scope = _viewsToScope[CurrentSourcePageType];

            var result = scope.LifetimeScope.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
            return result;
        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            _frameAdapter.BeginNavigationContext += FrameAdapterOnBeginNavigationContext;
            _frameAdapter.EndNavigationContext += FrameAdapterOnEndNavigationContext;
            _rootFrame.Navigating += RootFrameOnNavigating;
        }

        protected sealed override Frame CreateApplicationFrame()
        {
            if (_rootFrame == null)
            {
                _rootFrame = base.CreateApplicationFrame();
                _frameAdapter = new AutofacFrameAdapter(_rootFrame);
            }
            return _rootFrame;
        }

        private void FrameAdapterOnBeginNavigationContext(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                NavigationContext = e.Parameter;
            }
        }

        private void FrameAdapterOnEndNavigationContext(object sender, EventArgs e)
        {
            NavigationContext = null;
        }

        private Type _navigatingTo;
        private void RootFrameOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var page = _rootFrame.Content as Page;
            OnNavigating(e);
            if (!e.Cancel)
            {
                if (page != null && page.NavigationCacheMode == NavigationCacheMode.Disabled)
                {
                    //pages that have a cache mode of disabled won't be visited again
                    if (_viewsToScope.ContainsKey(e.SourcePageType))
                    {
                        OnViewModelDisposed(new ViewModelDisposedEventArgs(page.DataContext));
                        var scope = _viewsToScope[e.SourcePageType];
                        scope.LifetimeScope.Dispose();
                        _viewsToScope.Remove(e.SourcePageType);
                    }
                }

                var expired = new List<Type>();
                foreach (var scope in _viewsToScope)
                {
                    if (scope.Value.View.IsAlive == false)
                    {
                        expired.Add(scope.Key);
                        scope.Value.LifetimeScope.Dispose();
                    }
                }
                foreach (var key in expired)
                {
                    _viewsToScope.Remove(key);
                }
                SetupPreScope(e);
            }
        }

        /// <summary>
        /// Sets up a new lifetime scope as soon as the navigation service indicates we're going to another page
        /// </summary>
        private void SetupPreScope(NavigatingCancelEventArgs e)
        { 
            _navigatingTo = e.SourcePageType;
            if (!_viewsToScope.ContainsKey(_navigatingTo))
            {
                var scope = Container.BeginLifetimeScope();
                _viewsToScope.Add(_navigatingTo, new ViewScope(_framePlaceholder, scope));
            }
        }


        protected override void BuildUp(object instance)
        {
            Container.InjectProperties(instance);
        }

        public virtual void HandleConfigure(ContainerBuilder builder)
        {
        }

        public void OnActivated(IActivatedEventArgs<object> activation)
        {
            var handle = Activated;
            if (handle != null)
            {
                handle(activation.Instance);
            }
        }

        protected override void StartRuntime()
        {
            base.StartRuntime();
            OnNewSession();
        }

        private void OnNavigating(NavigatingCancelEventArgs e)
        {
            var handler = Navigating;
            if (handler != null) handler(this, e);
        }

        private void OnViewModelDisposed(ViewModelDisposedEventArgs e)
        {
            var handler = ViewModelDisposed;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnNewSession()
        {
            var handler = NewSession;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
