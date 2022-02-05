namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.ICustomTags" />
[FuryCoreService(true)]
internal class CustomTags : ICustomTags, IModService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomTags" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CustomTags(ConfigData config, IModServices services)
    {
        CustomTags.Instance = this;
        services.Lazy<IHarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{FuryCore.ModUniqueId}.{nameof(CustomTags)}";

                harmonyHelper.AddPatch(
                    id,
                    AccessTools.Method(typeof(Item), nameof(Item.GetContextTags)),
                    typeof(CustomTags),
                    nameof(CustomTags.Item_GetContextTags_Postfix),
                    PatchType.Postfix);

                harmonyHelper.ApplyPatches(id);
            });

        foreach (var customTag in config.CustomTags)
        {
            switch (customTag)
            {
                case CustomTag.CategoryArtifact:
                    this.AddContextTag(ICustomTags.CategoryArtifact, CustomTags.IsArtifact);
                    break;
                case CustomTag.CategoryFurniture:
                    this.AddContextTag(ICustomTags.CategoryFurniture, CustomTags.IsFurniture);
                    break;
                case CustomTag.DonateBundle:
                    this.AddContextTag(ICustomTags.DonateBundle, CustomTags.CanDonateToBundle);
                    break;
                case CustomTag.DonateMuseum:
                    this.AddContextTag(ICustomTags.DonateMuseum, CustomTags.CanDonateToMuseum);
                    break;
            }
        }
    }

    private static CustomTags Instance { get; set; }

    private IList<KeyValuePair<string, Func<Item, bool>>> Tags { get; } = new List<KeyValuePair<string, Func<Item, bool>>>();

    /// <inheritdoc />
    public void AddContextTag(string tag, Func<Item, bool> predicate)
    {
        this.Tags.Add(new(tag, predicate));
    }

    private static bool CanDonateToBundle(Item item)
    {
        return item is SObject obj
               && (Game1.locations
                        .OfType<CommunityCenter>()
                        .FirstOrDefault()?.couldThisIngredienteBeUsedInABundle(obj)
                   ?? false);
    }

    private static bool CanDonateToMuseum(Item item)
    {
        return Game1.locations
                    .OfType<LibraryMuseum>()
                    .FirstOrDefault()?.isItemSuitableForDonation(item)
               ?? false;
    }

    private static bool IsArtifact(Item item)
    {
        return item is SObject { Type: "Arch" };
    }

    private static bool IsFurniture(Item item)
    {
        return item is Furniture;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Item_GetContextTags_Postfix(Item __instance, ref HashSet<string> __result)
    {
        foreach (var (tag, predicate) in CustomTags.Instance.Tags)
        {
            try
            {
                if (!__result.Contains(tag) && predicate.Invoke(__instance))
                {
                    __result.Add(tag);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}