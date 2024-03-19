namespace StardewMods.OrdinaryCapsule.Framework.Models;

using System.Collections;
using System.Collections.Generic;
using StardewMods.Common.Integrations.OrdinaryCapsule;

/// <inheritdoc />
internal sealed class CapsuleItems : IList<ICapsuleItem>
{
    private readonly List<ICapsuleItem> capsuleItems = new();

    /// <inheritdoc />
    public int Count => this.capsuleItems.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public ICapsuleItem this[int index]
    {
        get => this.capsuleItems[index];
        set => this.capsuleItems[index] = value;
    }

    /// <inheritdoc />
    public void Add(ICapsuleItem item)
    {
        this.capsuleItems.Add(item);
    }

    /// <inheritdoc />
    public void Clear()
    {
        this.capsuleItems.Clear();
    }

    /// <inheritdoc />
    public bool Contains(ICapsuleItem item)
    {
        return this.capsuleItems.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(ICapsuleItem[] array, int arrayIndex)
    {
        this.capsuleItems.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public IEnumerator<ICapsuleItem> GetEnumerator()
    {
        return this.capsuleItems.GetEnumerator();
    }

    /// <inheritdoc />
    public int IndexOf(ICapsuleItem item)
    {
        return this.capsuleItems.IndexOf(item);
    }

    /// <inheritdoc />
    public void Insert(int index, ICapsuleItem item)
    {
        this.capsuleItems.Insert(index, item);
    }

    /// <inheritdoc />
    public bool Remove(ICapsuleItem item)
    {
        return this.capsuleItems.Remove(item);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        this.capsuleItems.RemoveAt(index);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.capsuleItems.GetEnumerator();
    }
}