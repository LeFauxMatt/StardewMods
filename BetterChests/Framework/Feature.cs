namespace StardewMods.BetterChests.Framework;

/// <summary>
///     Implementation of a Better Chest feature.
/// </summary>
internal abstract class Feature
{
    private bool isActivated;

    /// <summary>
    ///     Sets whether the feature is currently activated.
    /// </summary>
    /// <param name="value">A value indicating whether the feature is currently enabled.</param>
    public void SetActivated(bool value)
    {
        if (this.isActivated == value)
        {
            return;
        }

        this.isActivated = value;
        if (this.isActivated)
        {
            this.Activate();
            return;
        }

        this.Deactivate();
    }

    /// <summary>
    ///     Activate this feature.
    /// </summary>
    protected abstract void Activate();

    /// <summary>
    ///     Deactivate this feature.
    /// </summary>
    protected abstract void Deactivate();
}