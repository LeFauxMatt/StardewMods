namespace Common.Services
{
    using System;
    using System.Reflection;

    internal class PendingService
    {
        private readonly ConstructorInfo _constructor;

        public PendingService(Type serviceType)
        {
            this._constructor = serviceType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(ServiceManager),
                },
                null);
        }

        public IService Create(ServiceManager serviceManager)
        {
            return (IService)this._constructor.Invoke(
                new object[]
                {
                    serviceManager,
                });
        }
    }
}