using System;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Micro.Autofac
{
    public interface ISessionEvents
    {
        event EventHandler<NavigatingCancelEventArgs> Navigating;
        event EventHandler<ViewModelDisposedEventArgs> ViewModelDisposed;
        event EventHandler NewSession;
    }
}