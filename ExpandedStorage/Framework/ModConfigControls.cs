using System;
using StardewModdingAPI;

namespace ExpandedStorage.Framework
{
    internal class ModConfigControls
    {
        public string ScrollUp { get; set; } = $"{SButton.DPadUp}";
        public string ScrollDown { get; set; } = $"{SButton.DPadDown}";

        private SButton? _scrollUp;
        private SButton? _scrollDown;

        internal SButton? GetScrollUp
        {
            get {
                if (_scrollUp is { } sButton)
                    return sButton;
                else if (Enum.TryParse(ScrollUp, out SButton scrollUp))
                {
                    _scrollUp = scrollUp;
                    return scrollUp;
                }
                return null;
            }
        }

        internal SButton? GetScrollDown
        {
            get
            {
                if (_scrollDown is { } sButton)
                    return sButton;
                else if (Enum.TryParse(ScrollDown, out SButton scrollDown))
                {
                    _scrollDown = scrollDown;
                    return scrollDown;
                }
                return null;
            }
        }
    }
}