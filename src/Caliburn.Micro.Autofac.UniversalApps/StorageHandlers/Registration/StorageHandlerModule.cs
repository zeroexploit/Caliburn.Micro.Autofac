using System.Linq;
using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace Caliburn.Micro.Autofac.StorageHandlers.Registration
{
    public class StorageHandlerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(AssemblySource.Instance.Concat(new[] { typeof(IStorageMechanism).GetTypeInfo().Assembly }).ToArray())
                .AssignableTo<IStorageMechanism>()
                .AsImplementedInterfaces();

            builder.RegisterType<StorageCoordinator>().AsSelf().SingleInstance();

            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .AssignableTo<IStorageHandler>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<StartStorage>()
                .As<IStartable>()
                .SingleInstance();
        }

    }
}