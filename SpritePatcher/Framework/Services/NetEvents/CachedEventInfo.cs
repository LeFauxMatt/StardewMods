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
        private readonly Type eventHandlerType;
        private readonly EventInfo eventInfo;
        private readonly ConditionalWeakTable<object, HashSet<WeakReference<ISprite>>> handlers = new();
        private readonly Lazy<Delegate> handler;
        private readonly ILog log;

        private readonly
            ConditionalWeakTable<object, NetDictionary<Vector2, SObject, NetRef<SObject>,
                    SerializableDictionary<Vector2, SObject>, NetVector2Dictionary<SObject, NetRef<SObject>>>.
                ContentsChangeEvent> contentsChangedHandlers = new();

        private readonly ConditionalWeakTable<object, FieldChange<NetRef<Item>, Item>> itemFieldChangeHandlers = new();

        private readonly ConditionalWeakTable<object, FieldChange<NetRef<SObject>, SObject>> objectFieldChangeHandlers =
            new();

        /// <summary>Initializes a new instance of the <see cref="CachedEventInfo" /> class.</summary>
        /// <param name="log">Dependency used for logging debug information to the console.</param>
        /// <param name="eventInfo">The event info.</param>
        public CachedEventInfo(ILog log, EventInfo? eventInfo)
        {
            this.log = log;
            this.eventHandlerType = eventInfo?.EventHandlerType ?? throw new ArgumentNullException(nameof(eventInfo));
            this.eventInfo = eventInfo;
            this.handler = new Lazy<Delegate>(this.CreateHandler);
        }

        public void AddHandler(object source, ISprite target)
        {
            if (!this.handlers.TryGetValue(source, out var subscribers))
            {
                subscribers = new HashSet<WeakReference<ISprite>>();
                this.handlers.Add(source, subscribers);
            }

            if (!subscribers.Any())
            {
                switch (source)
                {
                    case NetRef<SObject> netRef when this.eventInfo.Name == "fieldChangeVisibleEvent":
                        netRef.fieldChangeVisibleEvent += this.GetObjectFieldChangeEvent(netRef);
                        break;
                    case NetRef<Item> netRef when this.eventInfo.Name == "fieldChangeVisibleEvent":
                        netRef.fieldChangeVisibleEvent += this.GetItemFieldChangeEvent(netRef);
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
                        this.eventInfo.AddEventHandler(source, this.handler.Value);
                        break;
                }
            }

            subscribers.Add(target.Self);
        }

        public void PublishEventOnce(object source)
        {
            switch (source)
            {
                case NetRef<SObject> netRef when this.eventInfo.Name == "fieldChangeVisibleEvent":
                    netRef.fieldChangeVisibleEvent -= this.GetObjectFieldChangeEvent(netRef);
                    break;
                case NetRef<Item> netRef when this.eventInfo.Name == "fieldChangeVisibleEvent":
                    netRef.fieldChangeVisibleEvent -= this.GetItemFieldChangeEvent(netRef);
                    break;
                case NetVector2Dictionary<SObject, NetRef<SObject>> netField when this.eventInfo.Name == "OnValueAdded":
                    netField.OnValueAdded -= this.GetContentsChangedEvent(netField);
                    break;
                case NetVector2Dictionary<SObject, NetRef<SObject>> netField
                    when this.eventInfo.Name == "OnValueRemoved":
                    netField.OnValueRemoved -= this.GetContentsChangedEvent(netField);
                    break;
                default:
                    this.eventInfo.RemoveEventHandler(source, this.handler.Value);
                    break;
            }

            if (!this.handlers.TryGetValue(source, out var subscribers))
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

        private NetDictionary<Vector2, SObject, NetRef<SObject>, SerializableDictionary<Vector2, SObject>,
            NetVector2Dictionary<SObject, NetRef<SObject>>>.ContentsChangeEvent GetContentsChangedEvent(
            NetVector2Dictionary<SObject, NetRef<SObject>> source)
        {
            if (this.contentsChangedHandlers.TryGetValue(source, out var contentsChanged))
            {
                return contentsChanged;
            }

            contentsChanged = (_, _) =>
            {
                this.PublishEventOnce(source);
            };

            this.contentsChangedHandlers.Add(source, contentsChanged);

            return contentsChanged;
        }

        private FieldChange<NetRef<Item>, Item> GetItemFieldChangeEvent(NetRef<Item> source)
        {
            if (this.itemFieldChangeHandlers.TryGetValue(source, out var fieldChangeHandler))
            {
                return fieldChangeHandler;
            }

            fieldChangeHandler = (_, _, newValue) =>
            {
                if (source.Value != null && object.ReferenceEquals(source.Value, newValue))
                {
                    this.PublishEventOnce(source);
                }
            };

            this.itemFieldChangeHandlers.Add(source, fieldChangeHandler);

            return fieldChangeHandler;
        }

        private FieldChange<NetRef<SObject>, SObject> GetObjectFieldChangeEvent(NetRef<SObject> source)
        {
            if (this.objectFieldChangeHandlers.TryGetValue(source, out var fieldChangeHandler))
            {
                return fieldChangeHandler;
            }

            fieldChangeHandler = (_, _, newValue) =>
            {
                if (source.Value != null && object.ReferenceEquals(source.Value, newValue))
                {
                    this.PublishEventOnce(source);
                }
            };

            this.objectFieldChangeHandlers.Add(source, fieldChangeHandler);

            return fieldChangeHandler;
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