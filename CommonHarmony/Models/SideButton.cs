namespace CommonHarmony.Models
{
    using System;
    using Enums;
    using StardewValley.Menus;

    internal class SideButton : IEquatable<SideButton>
    {
        public SideButton(ButtonType buttonType, ClickableTextureComponent button)
        {
            this.ButtonType = buttonType;
            this.Button = button;
        }

        public ButtonType ButtonType { get; }

        public ClickableTextureComponent Button { get; }

        public bool Equals(SideButton other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.ButtonType == other.ButtonType && Equals(this.Button, other.Button);
        }

        public static implicit operator SideButton(ClickableTextureComponent button)
        {
            return new(ButtonType.Custom, button);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == this.GetType() && this.Equals((SideButton)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)this.ButtonType * 397) ^ (this.Button != null ? this.Button.GetHashCode() : 0);
            }
        }
    }
}