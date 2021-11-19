namespace HelpForHire
{
    using Common.Services;
    using StardewModdingAPI;

    internal abstract class GenericChore : BaseService
    {
        private readonly string _translationKey;

        protected GenericChore(string translationKey, ServiceManager serviceManager)
            : base(translationKey)
        {
            this._translationKey = translationKey;
            this.Helper = serviceManager.Helper;
            this.IsActive = true;
        }

        /// <summary>Provides simplified APIs for writing mods.</summary>
        private protected IModHelper Helper { get; }

        /// <summary>The chore name.</summary>
        public string Name
        {
            get => this.Helper.Translation.Get($"chore.{this._translationKey}.name");
        }

        /// <summary>A description of what the chore does.</summary>
        public string Description
        {
            get => this.Helper.Translation.Get($"chore.{this._translationKey}.description");
        }

        /// <summary>Is the chore currently active.</summary>
        public bool IsActive { get; }

        /// <summary>Is the chore currently possible.</summary>
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
}