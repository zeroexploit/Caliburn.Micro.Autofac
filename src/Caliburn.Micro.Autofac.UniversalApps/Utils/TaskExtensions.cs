using System.Diagnostics;
using System.Threading.Tasks;

namespace Caliburn.Micro.Autofac.Utils
{
    public static class TaskExtensions
    {
        // Express explicit intention to not await for a task

        public static async void FireAndForget(this Task task)
        {
            if (task != null)
            {
                // await here anyway, not to get its result but to throw any exception in case
                await task;
            }
            else
            {
                Debug.WriteLine("WARNING: Trying to fire and forget a null Task");
            }
        }

        public static async void FireAndForget<T>(this Task<T> task)
        {
            if (task != null)
            {
                // await here anyway, not to get its result but to throw any exception in case
                await task;
            }
            else
            {
                Debug.WriteLine("WARNING: Trying to fire and forget a null Task<{0}>", typeof(T).Name);
            }
        }
    }
}
