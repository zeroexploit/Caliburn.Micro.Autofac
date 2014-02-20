using System;
using Autofac;

namespace Caliburn.Micro.Autofac
{
    internal class ViewScope : IEquatable<ViewScope>
    {
        private readonly object _view;
        private readonly ILifetimeScope _lifetimeScope;

        public object View
        {
            get { return _view; }
        }

        public ILifetimeScope LifetimeScope
        {
            get { return _lifetimeScope; }
        }

        public ViewScope(object view, ILifetimeScope lifetimeScope)
        {
            _view = view;
            _lifetimeScope = lifetimeScope;
        }

        public bool Equals(ViewScope other)
        {
            return ReferenceEquals(other.View, View);
        }
    }
}