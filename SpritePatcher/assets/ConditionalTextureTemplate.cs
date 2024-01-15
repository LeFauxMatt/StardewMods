namespace #REPLACE_namespace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;
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
    public Runner(IMonitor monitor, string modId, IContentPack contentPack, string target, Rectangle? sourceArea, List<DrawMethod> drawMethods, PatchMode patchMode)
        : base(monitor, modId, contentPack, target, sourceArea, drawMethods, patchMode) { }

    public override bool Run(IHaveModData entity)
    {
        this.Path = string.Empty;
        this.Area = Rectangle.Empty;
        this.Tint = Color.White;
        return this.ActualRun(entity);
    }

    public bool ActualRun(IHaveModData entity)
    {
        #REPLACE_code
    }
}