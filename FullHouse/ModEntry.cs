namespace FullHouse;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        I18n.Init(this.Helper.Translation);
    }
}