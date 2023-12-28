namespace StardewMods.ToolbarIcons.Framework.Interfaces;

/// <summary>Represents an integration with a method.</summary>
internal interface IMethodIntegration : ICustomIntegration
{
    /// <summary>Gets the unique mod id for the integration.</summary>
    string ModId { get; }

    /// <summary>Gets the method name for the integration.</summary>
    string MethodName { get; }

    /// <summary>Gets the arguments for the method.</summary>
    object?[] Arguments { get; }
}