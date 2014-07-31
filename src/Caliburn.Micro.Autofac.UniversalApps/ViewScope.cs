using System;
using Autofac;

namespace Caliburn.Micro.Autofac
{
    internal class ViewScope : IEquatable<ViewScope>
    {
        private readonly WeakReference _view;
        private readonly ILifetimeScope _lifetimeScope;

        public WeakReference View
        {
            get { return _view; }
        }

        public ILifetimeScope LifetimeScope
        {
            get { return _lifetimeScope; }
        }

        public ViewScope(object view, ILifetimeScope lifetimeScope)
        {
            _view = new WeakReference(view);
            _lifetimeScope = lifetimeScope;
        }

        public bool Equals(ViewScope other)
        {
            return ReferenceEquals(other.View.Target, View.Target);
        }
    }
}