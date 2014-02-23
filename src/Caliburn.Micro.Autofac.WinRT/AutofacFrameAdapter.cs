using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Micro.Autofac
{
    public class AutofacFrameAdapter : FrameAdapter
    {
        public event EventHandler<NavigationEventArgs> BeginNavigationContext = (s, e) => { };
        public event EventHandler EndNavigationContext = (s, e) => { };

        public AutofacFrameAdapter(Frame frame, bool treatViewAsLoaded = false) : base(frame, treatViewAsLoaded)
        {
        }

        protected override void OnNavigated(object sender, NavigationEventArgs e)
        {
            BeginNavigationContext(sender, e);
            try
            {
               base.OnNavigated(sender, e);
            }
            finally
            {
                EndNavigationContext(sender, EventArgs.Empty);
            }
        }
    }
}