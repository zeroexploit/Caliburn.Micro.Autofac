using System;
using Windows.UI.Xaml.Navigation;
using Autofac;

namespace Caliburn.Micro.Autofac.StorageHandlers.Registration
{
    internal class StartStorage : IStartable
    {
        private readonly StorageCoordinator _storageCoordinator;

        public StartStorage(StorageCoordinator storageCoordinator, ISessionEvents componentActivator)
        {
            _storageCoordinator = storageCoordinator;
            componentActivator.Navigating += ComponentActivatorOnNavigating;
            componentActivator.RootViewModelDisposed += ComponentActivatorOnRootViewModelDisposed;
            componentActivator.NewSession += ComponentActivatorOnNewSession;
        }

        private void ComponentActivatorOnNewSession(object sender, EventArgs eventArgs)
        {
            _storageCoordinator.ClearLastSession();
        }

        private void ComponentActivatorOnRootViewModelDisposed(object sender, ViewModelDisposedEventArgs e)
        {
            _storageCoordinator.RemoveInstance(e.Instance);
        }

        private void ComponentActivatorOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                _storageCoordinator.Save(StorageMode.Temporary);
            }
        }

        public void Start()
        {
            _storageCoordinator.Start();
        }
    }
}