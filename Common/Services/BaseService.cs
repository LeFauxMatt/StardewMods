namespace Common.Services
{
    /// <summary>
    ///     Encapsulates services that support the features of this mod.
    /// </summary>
    public abstract class BaseService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseService" /> class.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        private protected BaseService(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        /// <summary>Gets the name of the service.</summary>
        public string ServiceName { get; }
    }
}