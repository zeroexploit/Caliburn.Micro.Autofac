using Autofac;
using Autofac.Core;

namespace Caliburn.Micro.Autofac
{
    public class EventAggregationAutoSubscriptionModule : Module
    {

        static void OnComponentActivated(object sender, ActivatedEventArgs<object> args)
        {
            //  nothing we can do if a null event argument is passed (should never happen)
            if (args == null)
            {
                return;
            }
        }
    }
}
