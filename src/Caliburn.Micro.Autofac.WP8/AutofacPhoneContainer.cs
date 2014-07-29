using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using Autofac;
using Autofac.Core;
using System.Linq;

namespace Caliburn.Micro.Autofac
{
    internal class AutofacPhoneContainer : IPhoneContainer
    {
        private readonly IComponentContext context;

        public AutofacPhoneContainer(IComponentContext context)
        {
            this.context = context;
        }

        public event Action<object> Activated = _ => { };

        public void RegisterWithAppSettings(Type service, string appSettingsKey, Type implementation)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(appSettingsKey ?? service.FullName))
            {
                IsolatedStorageSettings.ApplicationSettings[appSettingsKey ?? service.FullName] = context.Resolve(implementation);
            }

            var builder = new ContainerBuilder();

            builder.Register(c =>
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(appSettingsKey ?? service.FullName))
                {
                    return IsolatedStorageSettings.ApplicationSettings[appSettingsKey ?? service.FullName];
                }

                return c.Resolve(implementation);
            }).Named(appSettingsKey, service);

            builder.Update(context.ComponentRegistry);
        }

        public void RegisterWithPhoneService(Type service, string phoneStateKey, Type implementation)
        {
            var pservice = (IPhoneService)GetInstance(typeof(IPhoneService), null);

            if (!pservice.State.ContainsKey(phoneStateKey ?? service.FullName))
            {
                pservice.State[phoneStateKey ?? service.FullName] = context.Resolve(implementation);
            }

            var builder = new ContainerBuilder();

            builder.Register(c =>
            {
                var phoneService = c.Resolve<IPhoneService>();

                if (phoneService.State.ContainsKey(phoneStateKey ?? service.FullName))
                {
                    return phoneService.State[phoneStateKey ?? service.FullName];
                }

                return c.Resolve(implementation);
            }).Named(phoneStateKey, service);

            builder.Update(context.ComponentRegistry);
        }

        private object GetInstance(Type service, string key)
        {
            try
            {
                object instance;
                if (string.IsNullOrEmpty(key))
                {
                    if (context.TryResolve(service, out instance))
                        return instance;
                }
                else
                {
                    //caliburn can ask for a Keyed service without providing the type,
                    //to fullfil this we must scan the actual component registry
                    if (service == null)
                    {
                        var unTyped = context.ComponentRegistry.Registrations.SelectMany(
                            x => x.Services.OfType<KeyedService>().Where(y => y.ServiceKey as string == key)).FirstOrDefault();
                        service = unTyped.ServiceType;
                    }

                    if (context.TryResolveNamed(key, service, out instance))
                        return instance;
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) Debugger.Break();
                throw;
            }

            throw new Exception(string.Format("Could not locate any instances of service {0}.", service.Name));
        }

        public void OnActivated(object instance)
        {
            var handle = this.Activated;
            if (handle != null)
                handle(instance);
        }
  }
}
