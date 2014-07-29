using System;

namespace Caliburn.Micro.Autofac
{
    public interface IActivateComponent
    {
        event Action<object> Activated;
    }
}