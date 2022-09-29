namespace StardewMods.BetterChests.Framework.Features;

using System;
using System.Globalization;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;

/// <summary>
///     Automatically organizes items between chests during sleep.
/// </summary>
internal sealed class AutoOrganize : IFeature
{
#nullable disable
    private static IFeature Instance;
#nullable enable

    private readonly IModHelper _helper;

    private bool _isActivated;

    private AutoOrganize(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <summary>
    ///     Initializes <see cref="AutoOrganize" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="AutoOrganize" /> class.</returns>
    public static IFeature Init(IModHelper helper)
    {
        return AutoOrganize.Instance ??= new AutoOrganize(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        this._helper.Events.GameLoop.DayEnding += AutoOrganize.OnDayEnding;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        this._helper.Events.GameLoop.DayEnding -= AutoOrganize.OnDayEnding;
    }

    private static void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        var storages = Storages.All.ToArray();
        Array.Sort(storages);

        foreach (var fromStorage in storages)
        {
            if (fromStorage.AutoOrganize is not FeatureOption.Enabled)
            {
                continue;
            }

            for (var index = fromStorage.Items.Count - 1; index >= 0; --index)
            {
                var item = fromStorage.Items[index];
                if (item is null)
                {
                    continue;
                }

                var stack = item.Stack;
                foreach (var toStorage in storages)
                {
                    if (ReferenceEquals(fromStorage, toStorage)
                     || fromStorage.StashToChestPriority >= toStorage.StashToChestPriority)
                    {
                        continue;
                    }

                    var tmp = toStorage.StashItem(item);
                    if (tmp is null)
                    {
                        Log.Trace(
                            $"AutoOrganize: {{ Item: {item.Name}, Quantity: {stack.ToString(CultureInfo.InvariantCulture)}, From: {fromStorage}, To: {toStorage}");
                        fromStorage.Items.Remove(item);
                        break;
                    }

                    if (stack != item.Stack)
                    {
                        Log.Trace(
                            $"AutoOrganize: {{ Item: {item.Name}, Quantity: {(stack - item.Stack).ToString(CultureInfo.InvariantCulture)}, From: {fromStorage}, To: {toStorage}");
                    }
                }
            }
        }

        foreach (var storage in storages)
        {
            storage.OrganizeItems();
        }
    }
}