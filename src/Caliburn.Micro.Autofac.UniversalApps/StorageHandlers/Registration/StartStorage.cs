using Autofac;

namespace Caliburn.Micro.Autofac.StorageHandlers.Registration
{
    internal class StartStorage : IStartable
    {
        private readonly StorageCoordinator _storageCoordinator;

        public StartStorage(StorageCoordinator storageCoordinator)
        {
            _storageCoordinator = storageCoordinator;
        }

        public void Start()
        {
            _storageCoordinator.Start();
        }
    }
}