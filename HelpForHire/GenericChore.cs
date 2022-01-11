namespace HelpForHire;

using StardewModdingAPI;

internal abstract class GenericChore : BaseService
{
    protected GenericChore(string translationKey, ServiceLocator serviceLocator)
        : base(translationKey)
    {
        this.Key = translationKey;
        this.Helper = serviceLocator.Helper;
        this.IsActive = true;
    }

    /// <summary>Provides simplified APIs for writing mods.</summary>
    private protected IModHelper Helper { get; }

    /// <summary>Gets the key name of the chore.</summary>
    public string Key { get; }

    /// <summary>Gets the chore name.</summary>
    public string Name
    {
        get => this.Helper.Translation.Get($"chore.{this.Key}.name");
    }

    /// <summary>Gets a description of what the chore does.</summary>
    public string Description
    {
        get => this.Helper.Translation.Get($"chore.{this.Key}.description");
    }

    /// <summary>Gets if the chore is currently active.</summary>
    public bool IsActive { get; }

    /// <summary>Gets if it is currently possible to perform the chore.</summary>
    public bool IsPossible
    {
        get => this.TestChore();
    }

    public void PerformChore()
    {
        this.DoChore();
    }

    protected abstract bool DoChore();
    protected abstract bool TestChore();
}