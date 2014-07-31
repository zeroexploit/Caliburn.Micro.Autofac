using System;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Micro.Autofac
{
    public interface ISessionEvents
    {
        event EventHandler<NavigatingCancelEventArgs> Navigating;
        event EventHandler<ViewModelDisposedEventArgs> RootViewModelDisposed;
        event EventHandler NewSession;
    }
}