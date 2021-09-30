namespace Common.Interfaces
{
    /// <summary>
    /// Service to handle creation/invocation of an event.
    /// </summary>
    /// <typeparam name="TEventArgs">The event argument type.</typeparam>
    internal interface IEventHandlerService<in TEventArgs>
    {
        /// <summary>
        /// Adds a handler for the event managed by this service.
        /// </summary>
        /// <param name="handler">The event handler to add.</param>
        void AddHandler(TEventArgs handler);

        /// <summary>
        /// Removed a handler for the event managed by this service.
        /// </summary>
        /// <param name="handler">The event handler to add.</param>
        void RemoveHandler(TEventArgs handler);
    }
}