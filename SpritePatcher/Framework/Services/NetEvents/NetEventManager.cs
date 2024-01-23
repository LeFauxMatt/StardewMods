namespace StardewMods.SpritePatcher.Framework.Services.NetEvents;

using System.Runtime.CompilerServices;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc cref="INetEventManager" />
internal sealed partial class NetEventManager : BaseService, INetEventManager
{
#nullable disable
    private static NetEventManager instance;
#nullable enable

    private readonly CachedEvents cachedEvents;
    private readonly ConditionalWeakTable<object, Type> cachedTypes = [];

    /// <summary>Initializes a new instance of the <see cref="NetEventManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public NetEventManager(ILog log, IManifest manifest)
        : base(log, manifest)
    {
        NetEventManager.instance = this;
        this.cachedEvents = new CachedEvents(this.Log);
    }

    /// <inheritdoc />
    public void Subscribe(ISprite target, object source, string eventName)
    {
        var type = NetEventManager.GetOrAddType(source);
        if (!this.cachedEvents.TryGetOrAddEvent(type, eventName, out var cachedEventInfo))
        {
            return;
        }

        cachedEventInfo.AddHandler(source, target);
    }

    private static void GenericHandler(object source, string eventName)
    {
        var type = NetEventManager.GetOrAddType(source);

        NetEventManager.instance.Log.Trace("Sending event from {0}.{1}.", source, eventName);
        if (!NetEventManager.instance.cachedEvents.TryGetEvent(type, eventName, out var cachedEventInfo))
        {
            return;
        }

        // Send event to subscribers
        cachedEventInfo.PublishEventOnce(source);
    }

    private static Type GetOrAddType(object source)
    {
        if (NetEventManager.instance.cachedTypes.TryGetValue(source, out var type))
        {
            return type;
        }

        type = source.GetType();
        NetEventManager.instance.cachedTypes.Add(source, type);
        return type;
    }
}