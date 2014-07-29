using System;

namespace Caliburn.Micro.Autofac
{
    public class ViewModelDisposedEventArgs : EventArgs
    {
        public object Instance { get; protected set; }

        public ViewModelDisposedEventArgs(object instance)
        {
            Instance = instance;
        }
    }
}