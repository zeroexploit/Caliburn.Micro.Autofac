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
        readonly IDictionary<WeakReference, ILifetimeScope> _viewsToScope = new Dictionary<WeakReference, ILifetimeScope>();
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

            if (_viewsToScope.Keys.Where(x => x.IsAlive).All(x => x.Target != view))
            {
                var scope = Container.BeginLifetimeScope(builder =>
                {
                    builder.RegisterInstance(view)
                        .AsSelf()
                        .AsImplementedInterfaces();
                    if (NavigationContext != null)
                    {
                        builder.RegisterInstance(NavigationContext)
                            .AsSelf()
                            .AsImplementedInterfaces();
                    }
                });
                _viewsToScope.Add(new WeakReference(view), scope);
            }

            return ViewModelLocator.LocateForViewType(view.GetType());
        }

        protected override object GetInstance(Type service, string key)
        {
            var weakKey = _viewsToScope.Keys.FirstOrDefault(x => x.IsAlive && x.Target == _rootFrame.Content);
            var scope = weakKey != null ? _viewsToScope[weakKey] : Container;

            try
            {
                object instance;
                if (string.IsNullOrEmpty(key))
                {
                    if (scope.TryResolve(service, out instance))
                        return instance;
                }
                else
                {
                    //caliburn can ask for a Keyed service without providing the type,
                    //to fullfil this we must scan the actual component registry
                    if (service == null)
                    {
                        var unTyped = Container.ComponentRegistry.Registrations.SelectMany(
                            x => x.Services.OfType<KeyedService>().Where(y => y.ServiceKey as string == key))
                                .FirstOrDefault();
                        service = unTyped.ServiceType;
                    }

                    if (scope.TryResolveNamed(key, service, out instance))
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

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var weakKey = _viewsToScope.Keys.FirstOrDefault(x => x.IsAlive && x.Target == _rootFrame.Content);
            var scope = weakKey != null ? _viewsToScope[weakKey] : Container;

            var result = scope.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
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

        private void RootFrameOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var page = _rootFrame.Content as Page;
            OnNavigating(e);
            if (!e.Cancel)
            {
                if (page != null && page.NavigationCacheMode == NavigationCacheMode.Disabled)
                {
                    //pages that have a cache mode of disabled won't be visited again
                    var key = _viewsToScope.Keys.SingleOrDefault(x => x.Target == page);
                    if (key != null)
                    {
                        OnViewModelDisposed(new ViewModelDisposedEventArgs(page.DataContext));
                        var scope = _viewsToScope[key];
                        scope.Dispose();
                        _viewsToScope.Remove(key);
                    }
                }

                var expired = new List<WeakReference>();
                foreach (var scope in _viewsToScope.Where(scope => scope.Key.IsAlive == false))
                {
                    expired.Add(scope.Key);
                    scope.Value.Dispose();
                }
                foreach (var key in expired)
                {
                    _viewsToScope.Remove(key);
                }
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
