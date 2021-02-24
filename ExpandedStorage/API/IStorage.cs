using System.Collections.Generic;

namespace ImJustMatt.ExpandedStorage.API
{
    public interface IStorage : IStorageConfig
    {
        /// <summary>The spritesheet to use for drawing this storage.</summary>
        string Image { get; set; }

        /// <summary>The number of animation frames in the spritesheet.</summary>
        int Frames { get; set; }

        /// <summary>Enables the player color choice and overlay layers.</summary>
        bool PlayerColor { get; set; }

        /// <summary>The depth from the bottom for the obstruction bounds.</summary>
        int Depth { get; set; }

        /// <summary>The game sound that will play when the storage is opened.</summary>
        string OpenSound { get; set; }

        /// <summary>The game sound that will play when the storage is placed.</summary>
        string PlaceSound { get; set; }

        /// <summary>One of the special chest types (None, MiniShippingBin, JunimoChest).</summary>
        string SpecialChestType { get; set; }

        /// <summary>Determines whether the storage type should be flagged as a Fridge.</summary>
        bool IsFridge { get; set; }

        /// <summary>Allows the storage to be placed in the world.</summary>
        bool IsPlaceable { get; set; }

        /// <summary>Add modData to placed chests (if key does not already exist).</summary>
        IDictionary<string, string> ModData { get; set; }

        /// <summary>When specified, storage may only hold items with allowed context tags.</summary>
        IList<string> AllowList { get; set; }

        /// <summary>When specified, storage may hold allowed items except for those with blocked context tags.</summary>
        IList<string> BlockList { get; set; }

        /// <summary>List of tabs to show on chest menu.</summary>
        IList<string> Tabs { get; set; }
    }
}