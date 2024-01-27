namespace StardewMods.SpritePatcher.Framework.Services.NetEvents;

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewValley.Network;

/// <inheritdoc cref="INetEventManager" />
internal sealed partial class NetEventManager
{
    /// <summary>Represents cached information about an event.</summary>
    private sealed class CachedEventInfo
    {
        private readonly ConditionalWeakTable<object, HashSet<WeakReference<ISprite>>> cachedSubscribers = new();
        private readonly Type eventHandlerType;
        private readonly EventInfo eventInfo;
        private readonly Lazy<Delegate> genericHandler;
        private readonly ConditionalWeakTable<object, object> handlers = new();
        private readonly ILog log;

        /// <summary>Initializes a new instance of the <see cref="CachedEventInfo" /> class.</summary>
        /// <param name="log">Dependency used for logging debug information to the console.</param>
        /// <param name="eventInfo">The event info.</param>
        public CachedEventInfo(ILog log, EventInfo? eventInfo)
        {
            this.log = log;
            this.eventHandlerType = eventInfo?.EventHandlerType ?? throw new ArgumentNullException(nameof(eventInfo));
            this.eventInfo = eventInfo;
            this.genericHandler = new Lazy<Delegate>(this.CreateHandler);
        }

        public void AddHandler(object source, ISprite target)
        {
            if (!this.cachedSubscribers.TryGetValue(source, out var subscribers))
            {
                subscribers = new HashSet<WeakReference<ISprite>>();
                this.cachedSubscribers.Add(source, subscribers);
            }

            if (!subscribers.Any())
            {
                switch (source)
                {
                    case NetRef<INetObject<INetSerializable>> netRef
                        when this.eventInfo.Name == "fieldChangeVisibleEvent":
                        netRef.fieldChangeVisibleEvent += this.GetFieldChangeEvent(netRef);
                        break;
                    case NetVector2Dictionary<SObject, NetRef<SObject>> netField
                        when this.eventInfo.Name == "OnValueAdded":
                        netField.OnValueAdded += this.GetContentsChangedEvent(netField);
                        break;
                    case NetVector2Dictionary<SObject, NetRef<SObject>> netField
                        when this.eventInfo.Name == "OnValueRemoved":
                        netField.OnValueRemoved += this.GetContentsChangedEvent(netField);
                        break;
                    default:
                        this.eventInfo.AddEventHandler(source, this.genericHandler.Value);
                        break;
                }
            }

            subscribers.Add(target.Self);
        }

        public void PublishEventOnce(object source)
        {
            switch (source)
            {
                case NetRef<INetObject<INetSerializable>> netRef when this.eventInfo.Name == "fieldChangeVisibleEvent":
                    netRef.fieldChangeVisibleEvent -= this.GetFieldChangeEvent(netRef);
                    break;
                case NetVector2Dictionary<SObject, NetRef<SObject>> netField when this.eventInfo.Name == "OnValueAdded":
                    netField.OnValueAdded -= this.GetContentsChangedEvent(netField);
                    break;
                case NetVector2Dictionary<SObject, NetRef<SObject>> netField
                    when this.eventInfo.Name == "OnValueRemoved":
                    netField.OnValueRemoved -= this.GetContentsChangedEvent(netField);
                    break;
                default:
                    this.eventInfo.RemoveEventHandler(source, this.genericHandler.Value);
                    break;
            }

            if (!this.cachedSubscribers.TryGetValue(source, out var subscribers))
            {
                return;
            }

            foreach (var subscriber in subscribers)
            {
                if (subscriber.TryGetTarget(out var sprite))
                {
                    sprite.ClearCache();
                }
            }

            subscribers.Clear();
        }

        private NetDictionary<Vector2, T, NetRef<T>, SerializableDictionary<Vector2, T>,
            NetVector2Dictionary<T, NetRef<T>>>.ContentsChangeEvent GetContentsChangedEvent<T>(
            NetVector2Dictionary<T, NetRef<T>> source)
            where T : class, INetObject<INetSerializable>
        {
            if (this.handlers.TryGetValue(source, out var handler))
            {
                return (handler as NetDictionary<Vector2, T, NetRef<T>, SerializableDictionary<Vector2, T>,
                    NetVector2Dictionary<T, NetRef<T>>>.ContentsChangeEvent)!;
            }

            handler =
                new NetDictionary<Vector2, T, NetRef<T>, SerializableDictionary<Vector2, T>,
                    NetVector2Dictionary<T, NetRef<T>>>.ContentsChangeEvent(
                    (_, _) =>
                    {
                        this.PublishEventOnce(source);
                    });

            this.handlers.Add(source, handler);

            return (handler as NetDictionary<Vector2, T, NetRef<T>, SerializableDictionary<Vector2, T>,
                NetVector2Dictionary<T, NetRef<T>>>.ContentsChangeEvent)!;
        }

        private FieldChange<NetRef<T>, T> GetFieldChangeEvent<T>(NetRef<T> source)
            where T : class, INetObject<INetSerializable>
        {
            if (this.handlers.TryGetValue(source, out var handler))
            {
                return (handler as FieldChange<NetRef<T>, T>)!;
            }

            handler = new FieldChange<NetRef<T>, T>(
                (_, _, newValue) =>
                {
                    if (source.Value != null && ReferenceEquals(source.Value, newValue))
                    {
                        this.PublishEventOnce(source);
                    }
                });

            this.handlers.Add(source, handler);
            return (handler as FieldChange<NetRef<T>, T>)!;
        }

        private Delegate CreateHandler()
        {
            var invokeMethod = this.eventHandlerType.GetMethod("Invoke");
            if (invokeMethod == null)
            {
                throw new InvalidOperationException("Event handler type does not have an Invoke method.");
            }

            var dynamicMethod = new DynamicMethod(
                $"CachedEventInfo_{this.eventInfo.Name}",
                invokeMethod.ReturnType,
                invokeMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                typeof(NetEventManager).Module);

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, this.eventInfo.Name);
            il.Emit(
                OpCodes.Call,
                AccessTools.DeclaredMethod(typeof(NetEventManager), nameof(NetEventManager.GenericHandler)));

            il.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(this.eventHandlerType);
        }
    }
}