namespace StardewMods.SmackDatScarecrow;

using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley.Extensions;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
#nullable disable
    private static ModEntry instance;
#nullable enable

    private IReflectedField<Multiplayer>? multiplayer;

    private static Multiplayer Multiplayer => ModEntry.instance.multiplayer!.GetValue();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ModEntry.instance = this;
        I18n.Init(this.Helper.Translation);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        // Patches
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools
                .GetDeclaredMethods(typeof(GameLocation))
                .Single(method => method.Name == "damageMonster" && method.GetParameters().Length >= 10),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_damageMonster_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void GameLocation_damageMonster_postfix(
        GameLocation __instance,
        ref bool __result,
        Rectangle areaOfEffect,
        int minDamage,
        int maxDamage,
        float critChance,
        float critMultiplier,
        Farmer who)
    {
        if (__result || __instance is not Farm farm)
        {
            return;
        }

        foreach (var (_, obj) in farm.Objects.Pairs)
        {
            if (!obj.HasTypeBigCraftable() || !obj.IsScarecrow() || who is null)
            {
                continue;
            }

            var monsterBox = obj.GetBoundingBox();
            if (!monsterBox.Intersects(areaOfEffect))
            {
                continue;
            }

            if (Game1.currentLocation.Equals(farm))
            {
                Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 8.0), 200 + Game1.random.Next(-50, 50));
            }

            var crit = false;
            int damageAmount;
            if (who.professions.Contains(25))
            {
                critChance += critChance * 0.5f;
            }

            if (maxDamage >= 0)
            {
                damageAmount = Game1.random.Next(minDamage, maxDamage + 1);
                if (Game1.random.NextDouble() < critChance + (who.LuckLevel * (critChance / 40f)))
                {
                    crit = true;
                    farm.playSound("crit");
                }

                damageAmount = crit ? (int)(damageAmount * critMultiplier) : damageAmount;
                damageAmount = Math.Max(1, damageAmount + (who.Attack * 3));

                if (who.professions.Contains(24))
                {
                    damageAmount = (int)Math.Ceiling(damageAmount * 1.1f);
                }

                if (who.professions.Contains(26))
                {
                    damageAmount = (int)Math.Ceiling(damageAmount * 1.15f);
                }

                if (crit && who.professions.Contains(29))
                {
                    damageAmount *= 2;
                }

                var debris = new Debris(
                    damageAmount,
                    new Vector2(monsterBox.Center.X + 16, monsterBox.Center.Y),
                    crit ? Color.Yellow : new Color(255, 130, 0),
                    crit ? 1f + (damageAmount / 300f) : 1f,
                    Game1.player);

                debris.Chunks[0].scale = Math.Min(2f, Math.Max(1f, crit ? 1f + (damageAmount / 300f) : 1f));
                debris.Chunks[0].xVelocity.Value = Game1.random.Next(-1, 2);
                farm.debris.Add(debris);
            }
            else
            {
                damageAmount = -2;
            }

            if (who.CurrentTool is not null
                && who.CurrentTool.Name.Equals("Galaxy Sword", StringComparison.OrdinalIgnoreCase))
            {
                ModEntry.Multiplayer.broadcastSprites(
                    farm,
                    new TemporaryAnimatedSprite(
                        362,
                        Game1.random.Next(50, 120),
                        6,
                        1,
                        new Vector2(monsterBox.Center.X - 32, monsterBox.Center.Y - 32),
                        false,
                        false));
            }

            if (damageAmount > 0 && crit is true and true)
            {
                var standPos = new Vector2(monsterBox.Center.X, monsterBox.Center.Y);
                ModEntry.Multiplayer.broadcastSprites(
                    farm,
                    new TemporaryAnimatedSprite(
                        362,
                        Game1.random.Next(15, 50),
                        6,
                        1,
                        standPos - new Vector2(32f, 32f),
                        false,
                        Game1.random.NextDouble() < 0.5)
                    {
                        scale = 0.75f,
                        alpha = crit ? 0.75f : 0.5f,
                    });

                ModEntry.Multiplayer.broadcastSprites(
                    farm,
                    new TemporaryAnimatedSprite(
                        362,
                        Game1.random.Next(15, 50),
                        6,
                        1,
                        standPos - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)),
                        false,
                        Game1.random.NextDouble() < 0.5)
                    {
                        scale = 0.5f,
                        delayBeforeAnimationStart = 50,
                        alpha = crit ? 0.75f : 0.5f,
                    });

                ModEntry.Multiplayer.broadcastSprites(
                    farm,
                    new TemporaryAnimatedSprite(
                        362,
                        Game1.random.Next(15, 50),
                        6,
                        1,
                        standPos - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)),
                        false,
                        Game1.random.NextDouble() < 0.5)
                    {
                        scale = 0.5f,
                        delayBeforeAnimationStart = 100,
                        alpha = crit ? 0.75f : 0.5f,
                    });

                ModEntry.Multiplayer.broadcastSprites(
                    farm,
                    new TemporaryAnimatedSprite(
                        362,
                        Game1.random.Next(15, 50),
                        6,
                        1,
                        standPos - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)),
                        false,
                        Game1.random.NextDouble() < 0.5)
                    {
                        scale = 0.5f,
                        delayBeforeAnimationStart = 150,
                        alpha = crit ? 0.75f : 0.5f,
                    });

                ModEntry.Multiplayer.broadcastSprites(
                    farm,
                    new TemporaryAnimatedSprite(
                        362,
                        Game1.random.Next(15, 50),
                        6,
                        1,
                        standPos - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)),
                        false,
                        Game1.random.NextDouble() < 0.5)
                    {
                        scale = 0.5f,
                        delayBeforeAnimationStart = 200,
                        alpha = crit ? 0.75f : 0.5f,
                    });
            }

            obj.shakeTimer = 100;
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) =>
        this.multiplayer = this.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
}