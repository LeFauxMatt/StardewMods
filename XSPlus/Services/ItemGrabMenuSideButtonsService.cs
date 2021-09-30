namespace XSPlus.Services
{
    using StardewModdingAPI.Utilities;
    using StardewValley.Menus;

    /// <inheritdoc />
    internal class ItemGrabMenuSideButtonsService : BaseService
    {
        private static ItemGrabMenuSideButtonsService Instance;
        private readonly PerScreen<ClickableTextureComponent> _buttons = new();

        private ItemGrabMenuSideButtonsService()
            : base("ItemGrabMenuSideButtons")
        {
        }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="ItemGrabMenuSideButtonsService"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="ItemGrabMenuSideButtonsService"/> class.</returns>
        public static ItemGrabMenuSideButtonsService GetSingleton(ServiceManager serviceManager)
        {
            return ItemGrabMenuSideButtonsService.Instance ??= new ItemGrabMenuSideButtonsService();
        }
    }
}