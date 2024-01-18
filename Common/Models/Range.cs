namespace StardewMods.Common.Models;

/// <summary>Represents a range of values.</summary>
/// <typeparam name="T">The value type for the range.</typeparam>
internal sealed class Range<T>
    where T : IComparable<T>
{
    /// <summary>Initializes a new instance of the <see cref="Range{T}" /> class.</summary>
    public Range()
    {
        this.Minimum = default(T?)!;
        this.Maximum = default(T)!;
    }

    /// <summary>Initializes a new instance of the <see cref="Range{T}" /> class.</summary>
    /// <param name="minimum">The minimum value of the range.</param>
    /// <param name="maximum">The maximum value of the range.</param>
    public Range(T minimum, T maximum)
    {
        this.Minimum = minimum;
        this.Maximum = maximum;
    }

    /// <summary>Gets or sets the maximum value of the range.</summary>
    public T? Maximum { get; set; }

    /// <summary>Gets or sets the minimum value of the range.</summary>
    public T? Minimum { get; set; }

    /// <summary>Clamps a value based on the range.</summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>Returns the clamped value.</returns>
    /// <exception cref="InvalidOperationException">Range is not valid.</exception>
    public T Clamp(T value)
    {
        if (!this.IsValid() || this.Minimum is null || this.Maximum is null)
        {
            throw new InvalidOperationException();
        }

        if (value.CompareTo(this.Minimum) <= 0)
        {
            return this.Minimum;
        }

        return this.Maximum.CompareTo(value) <= 0 ? this.Maximum : value;
    }

    private bool IsValid() =>
        this.Minimum is not null && this.Maximum is not null && this.Minimum.CompareTo(this.Maximum) <= 0;
}