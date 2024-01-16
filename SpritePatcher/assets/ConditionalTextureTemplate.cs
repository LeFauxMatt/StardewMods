namespace #REPLACE_namespace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Models;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using IHaveModData = StardewValley.IHaveModData;
using SObject = StardewValley.Object;

public class Runner : global::StardewMods.SpritePatcher.BasePatchModel
{
    public Runner(PatchModelCtorArgs args)
        : base(args) { }

    public override bool Run(IHaveModData entity)
    {
        this.Texture = null;
        this.Area = Rectangle.Empty;
        this.Tint = null;
        this.ActualRun(entity);
        if (this.Texture is null) return false;
        this.Area ??= new Rectangle(0, 0, this.Texture.Width, this.Texture.Height);
        if (this.Area.Value.Right > this.Texture.Width || this.Area.Value.Bottom > this.Texture.Height) return false;
        return true;
    }

    public void ActualRun(IHaveModData entity)
    {
#REPLACE_code
    }
}