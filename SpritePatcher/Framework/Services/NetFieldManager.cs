// namespace StardewMods.SpritePatcher.Framework.Services;
//
// using System.Reflection;
// using Netcode;
// using StardewMods.Common.Services;
// using StardewMods.Common.Services.Integrations.FuryCore;
// using StardewMods.SpritePatcher.Framework.Models;
//
// internal sealed class NetFieldManager : BaseService
// {
//     private static readonly Dictionary<Type, IDictionary<string, (INetSerializable NetField, EventInfo? EventInfo)>>
//         CachedEvents = [];
//
//     private readonly Dictionary<string, (INetSerializable Target, Delegate Handler)> subscribedEvents =
//         new(StringComparer.OrdinalIgnoreCase);
//
//     private readonly Dictionary<string, HashSet<TextureKey>> fieldTargets = new(StringComparer.OrdinalIgnoreCase);
//
//     public NetFieldManager(ILog log, IManifest manifest)
//         : base(log, manifest) { }
//
//     private void SubscribeToFieldEvent(string name, TextureKey key)
//     {
//         if (!ManagedObject.CachedEvents.TryGetValue(this.type, out var objectEvents))
//         {
//             objectEvents = new Dictionary<string, (INetSerializable NetField, EventInfo? EventInfo)>();
//             ManagedObject.CachedEvents[this.type] = objectEvents;
//         }
//
//         if (this.entity is not INetObject<NetFields> obj)
//         {
//             return;
//         }
//
//         // Create targets for field if they dont' exist
//         var fieldName = obj.NetFields.Name + ": " + name;
//         if (!this.fieldTargets.TryGetValue(fieldName, out var targets))
//         {
//             targets = new HashSet<TextureKey>();
//             this.fieldTargets[fieldName] = targets;
//         }
//
//         // Check if already subscribed to event and add target
//         if (this.subscribedEvents.ContainsKey(fieldName))
//         {
//             targets.Add(key);
//             return;
//         }
//
//         // Check if cached event info exists
//         if (!objectEvents.TryGetValue(fieldName, out var objectEvent))
//         {
//             foreach (var field in obj.NetFields.GetFields())
//             {
//                 // Check if field name matches
//                 if (!field.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
//                 {
//                     continue;
//                 }
//
//                 var fieldType = field.GetType();
//                 var eventInfo = fieldType.GetEvent("fieldChangeVisibleEvent");
//                 objectEvents[fieldName] = (field, eventInfo);
//                 break;
//             }
//         }
//
//         if (objectEvent.EventInfo?.EventHandlerType == null)
//         {
//             return;
//         }
//
//         // Create a delegate for the event
//         EventHandler eventHandler = (_, _) => this.ClearCache(targets.Select(t => t.Target));
//
//         objectEvent.EventInfo.AddEventHandler(objectEvent.NetField, eventHandler);
//         this.subscribedEvents[fieldName] = (objectEvent.NetField, eventHandler);
//         targets.Add(key);
//     }
//
//     private void UnsubscribeFromFieldEvents(TextureKey key)
//     {
//         if (!ManagedObject.CachedEvents.TryGetValue(this.type, out var objectEvents))
//         {
//             return;
//         }
//
//         foreach (var (fieldName, (netField, eventInfo)) in objectEvents)
//         {
//             if (!this.fieldTargets.TryGetValue(fieldName, out var targets))
//             {
//                 continue;
//             }
//
//             // Remove this particular target
//             targets.Remove(key);
//             if (targets.Any())
//             {
//                 continue;
//             }
//
//             // If no targets remain, then unsubscribe from the actual event
//             if (eventInfo?.EventHandlerType == null
//                 || !this.subscribedEvents.TryGetValue(fieldName, out var subscribedEvent))
//             {
//                 continue;
//             }
//
//             eventInfo.RemoveEventHandler(netField, subscribedEvent.Handler);
//             this.subscribedEvents.Remove(fieldName);
//         }
//     }
// }