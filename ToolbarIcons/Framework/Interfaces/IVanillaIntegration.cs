namespace StardewMods.ToolbarIcons.Framework.Interfaces;

/// <summary>Represents an integration for a vanilla method.</summary>
internal interface IVanillaIntegration : ICustomIntegration
{
    /// <summary>Performs an action.</summary>
    public void DoAction();
}