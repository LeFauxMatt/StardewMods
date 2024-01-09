namespace StardewMods.Common.Models;

/// <summary>
/// Represents an attribute used to specify the priority of a subscriber method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PriorityAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityAttribute"/> class.
    /// </summary>
    /// <param name="priority">The priority level for the subscriber.</param>
    public PriorityAttribute(int priority) => this.Priority = priority;

    public int Priority { get; private set; }
}