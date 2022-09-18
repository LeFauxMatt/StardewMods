namespace StardewMods.SmackDatScarecrow;

using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;

/// <inheritdoc />
public sealed class SmackDatScarecrow : Mod
{
#nullable disable
    private static SmackDatScarecrow Instance;
#nullable enable

    private IReflectedField<Multiplayer>? _multiplayer;

    private static Multiplayer Multiplayer => SmackDatScarecrow.Instance._multiplayer!.GetValue();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        SmackDatScarecrow.Instance = this;
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        // Patches
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(
                typeof(GameLocation),
                nameof(GameLocation.damageMonster),
                new[]
                {
                    typeof(Rectangle),
                    typeof(int),
                    typeof(int),
                    typeof(bool),
                    typeof(float),
                    typeof(int),
                    typeof(float),
                    typeof(float),
                    typeof(bool),
                    typeof(Farmer),
                }),
            postfix: new(typeof(SmackDatScarecrow), nameof(SmackDatScarecrow.GameLocation_damageMonster_postfix)));
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

        foreach (var (pos, obj) in farm.Objects.Pairs)
        {
            if (obj is not { bigCraftable.Value: true } || !obj.IsScarecrow() || who is null)
            {
                continue;
            }

            var monsterBox = obj.getBoundingBox(pos);
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
                if (Game1.random.NextDouble() < critChance + who.LuckLevel * (critChance / 40f))
                {
                    crit = true;
                    farm.playSound("crit");
                }

                damageAmount = crit ? (int)(damageAmount * critMultiplier) : damageAmount;
                damageAmount = Math.Max(1, damageAmount + who.attack * 3);

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

                var debris = new Debris(-1, 1, new(monsterBox.Right, monsterBox.Bottom), Game1.player.Position)
                {
                    chunkType = { Value = damageAmount },
                    debrisType = { Value = Debris.DebrisType.NUMBERS },
                    nonSpriteChunkColor = { Value = crit ? Color.Yellow : new(255, 130, 0) },
                };

                debris.Chunks[0].scale = Math.Min(2f, Math.Max(1f, crit ? 1f + damageAmount / 300f : 1f));
                debris.Chunks[0].xVelocity.Value = Game1.random.Next(-1, 2);
                farm.debris.Add(debris);
            }
            else
            {
                damageAmount = -2;
            }

            if (who.CurrentTool?.Name.Equals("Galaxy Sword") == true)
            {
                SmackDatScarecrow.Multiplayer.broadcastSprites(
                    farm,
                    new TemporaryAnimatedSprite(
                        362,
                        Game1.random.Next(50, 120),
                        6,
                        1,
                        new(monsterBox.Center.X - 32, monsterBox.Center.Y - 32),
                        false,
                        false));
            }

            if (damageAmount > 0 && crit is true and true)
            {
                var standPos = new Vector2(monsterBox.Center.X, monsterBox.Center.Y);
                SmackDatScarecrow.Multiplayer.broadcastSprites(
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
                SmackDatScarecrow.Multiplayer.broadcastSprites(
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
                SmackDatScarecrow.Multiplayer.broadcastSprites(
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
                SmackDatScarecrow.Multiplayer.broadcastSprites(
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
                SmackDatScarecrow.Multiplayer.broadcastSprites(
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

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this._multiplayer = this.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
    }
}