namespace StardewMods.SpritePatcher.Framework.Services;

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc cref="StardewMods.SpritePatcher.Framework.Interfaces.INetFieldManager" />
internal sealed class NetFieldManager : BaseService, INetFieldManager
{
#nullable disable
    private static NetFieldManager instance;
#nullable enable

    private readonly
        Dictionary<Type, Dictionary<string, (EventInfo? EventInfo, Delegate? Handler,
            ConditionalWeakTable<object, HashSet<IManagedObject>> Handlers)>> cachedEvents = [];

    /// <summary>Initializes a new instance of the <see cref="NetFieldManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public NetFieldManager(ILog log, IManifest manifest)
        : base(log, manifest) =>
        NetFieldManager.instance = this;

    /// <inheritdoc />
    public void SubscribeToFieldEvent(IManagedObject target, object source, string eventName)
    {
        var type = source.GetType();
        if (!this.cachedEvents.TryGetValue(type, out var eventsBySourceType))
        {
            eventsBySourceType =
                new Dictionary<string, (EventInfo? EventInfo, Delegate? Handler,
                    ConditionalWeakTable<object, HashSet<IManagedObject>> Handlers)>(StringComparer.OrdinalIgnoreCase);

            this.cachedEvents[type] = eventsBySourceType;
        }

        if (!eventsBySourceType.TryGetValue(eventName, out var eventInfoByEventName))
        {
            eventInfoByEventName.EventInfo = type.GetEvent(eventName);
            eventInfoByEventName.Handlers = new ConditionalWeakTable<object, HashSet<IManagedObject>>();
            eventsBySourceType[eventName] = eventInfoByEventName;
        }

        if (eventInfoByEventName.EventInfo?.EventHandlerType == null)
        {
            return;
        }

        // Get Delegate from cache or generate a new one through ilGenerator
        if (eventInfoByEventName.Handler is null)
        {
            var invokeMethod = eventInfoByEventName.EventInfo.EventHandlerType.GetMethod("Invoke");
            if (invokeMethod == null)
            {
                return;
            }

            var dynamicMethod = new DynamicMethod(
                $"NetFieldManager_{eventInfoByEventName.EventInfo.Name}",
                invokeMethod.ReturnType,
                invokeMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                typeof(NetFieldManager).Module);

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, eventName);
            il.Emit(
                OpCodes.Call,
                AccessTools.DeclaredMethod(typeof(NetFieldManager), nameof(NetFieldManager.GenericHandler)));

            il.Emit(OpCodes.Ret);

            eventInfoByEventName.Handler =
                dynamicMethod.CreateDelegate(eventInfoByEventName.EventInfo.EventHandlerType);
        }

        eventInfoByEventName.EventInfo.AddEventHandler(source, eventInfoByEventName.Handler);

        if (!eventInfoByEventName.Handlers.TryGetValue(source, out var subscribers))
        {
            subscribers = new HashSet<IManagedObject>();
            eventInfoByEventName.Handlers.Add(source, subscribers);
        }

        subscribers.Add(target);
    }

    private static void GenericHandler(object source, string eventName)
    {
        var type = source.GetType();
        NetFieldManager.instance.Log.Trace("Sending event from {0}.{1}.", source, eventName);
        if (!NetFieldManager.instance.cachedEvents.TryGetValue(type, out var eventsBySourceType))
        {
            return;
        }

        if (!eventsBySourceType.TryGetValue(eventName, out var eventInfoByEventName))
        {
            return;
        }

        // Send event to subscribers
        if (eventInfoByEventName.Handlers.TryGetValue(source, out var subscribers))
        {
            foreach (var subscriber in subscribers)
            {
                subscriber.ClearCache();
            }

            subscribers.Clear();
        }

        if (eventInfoByEventName.EventInfo == null || eventInfoByEventName.Handler == null)
        {
            return;
        }

        // Unsubscribe handler from event
        eventInfoByEventName.EventInfo.RemoveEventHandler(source, eventInfoByEventName.Handler);
    }
}