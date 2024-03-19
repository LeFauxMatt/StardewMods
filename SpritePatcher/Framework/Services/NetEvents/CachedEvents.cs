namespace StardewMods.SpritePatcher.Framework.Services.NetEvents;

using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc cref="INetEventManager" />
internal sealed partial class NetEventManager
{
    /// <summary>Represents a cache of events.</summary>
    private sealed class CachedEvents
    {
        private readonly Dictionary<Type, Dictionary<string, CachedEventInfo?>> events = new();
        private readonly ILog log;

        /// <summary>Initializes a new instance of the <see cref="CachedEvents" /> class.</summary>
        /// <param name="log">Dependency used for logging debug information to the console.</param>
        public CachedEvents(ILog log) => this.log = log;

        /// <summary>Gets or adds the cached information about an event.</summary>
        /// <param name="type">The event's declaring type.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="cachedEventInfo">
        /// When this method returns, contains the cached event information, if the event is found or
        /// added; otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if the event is found or added; otherwise, <c>false</c>.</returns>
        public bool TryGetOrAddEvent(
            Type type,
            string eventName,
            [NotNullWhen(true)] out CachedEventInfo? cachedEventInfo)
        {
            cachedEventInfo = null;
            var eventsBySourceType = this.GetOrAddEventsBySourceType(type);
            if (eventsBySourceType.TryGetValue(eventName, out cachedEventInfo))
            {
                return cachedEventInfo is not null;
            }

            return this.TryAddEvent(type, eventName, eventsBySourceType, out cachedEventInfo);
        }

        /// <summary>Tries to get the specified event from the given type and event name.</summary>
        /// <param name="type">The type from which to get the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="cachedEventInfo">
        /// When this method returns, contains the cached event information, if the event is found;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if the event is found; otherwise, <c>false</c>.</returns>
        public bool TryGetEvent(Type type, string eventName, [NotNullWhen(true)] out CachedEventInfo? cachedEventInfo)
        {
            cachedEventInfo = null;
            return this.events.TryGetValue(type, out var eventsBySourceType)
                && eventsBySourceType.TryGetValue(eventName, out cachedEventInfo);
        }

        private Dictionary<string, CachedEventInfo?> GetOrAddEventsBySourceType(Type type)
        {
            if (this.events.TryGetValue(type, out var eventsBySourceType))
            {
                return eventsBySourceType;
            }

            eventsBySourceType = new Dictionary<string, CachedEventInfo?>(StringComparer.OrdinalIgnoreCase);
            this.events[type] = eventsBySourceType;
            return eventsBySourceType;
        }

        private bool TryAddEvent(
            Type type,
            string eventName,
            IDictionary<string, CachedEventInfo?> eventsBySourceType,
            [NotNullWhen(true)] out CachedEventInfo? cachedEventInfo)
        {
            var eventInfo = type.GetEvent(eventName);
            if (eventInfo?.EventHandlerType != null)
            {
                cachedEventInfo = new CachedEventInfo(this.log, eventInfo);
                eventsBySourceType[eventName] = cachedEventInfo;
                return true;
            }

            this.log.Warn("Event handler type for {0} is null.", eventName);
            eventsBySourceType[eventName] = null;
            cachedEventInfo = null;
            return false;
        }
    }
}