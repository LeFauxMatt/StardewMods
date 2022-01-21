namespace FuryCore.Attributes;

using System;

/// <inheritdoc />
[AttributeUsage(AttributeTargets.Class)]
internal class FuryCoreServiceAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FuryCoreServiceAttribute" /> class.
    /// </summary>
    /// <param name="exportable"></param>
    public FuryCoreServiceAttribute(bool exportable)
    {
        this.Exportable = exportable;
    }

    public bool Exportable { get; }
}