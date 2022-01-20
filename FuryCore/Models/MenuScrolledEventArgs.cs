namespace FuryCore.Models;

using System;

/// <summary>
/// 
/// </summary>
public class MenuScrolledEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuScrolledEventArgs"/> class.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rows"></param>
    public MenuScrolledEventArgs(int position, int rows)
    {
        this.Position = position;
        this.Rows = rows;
    }

    /// <summary>
    /// 
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// 
    /// </summary>
    public int Rows { get; }
}