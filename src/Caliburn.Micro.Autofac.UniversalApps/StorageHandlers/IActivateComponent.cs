using System;

namespace Caliburn.Micro.Autofac.StorageHandlers
{
    public interface IActivateComponent
    {
        event Action<object> Activated;
    }
}