namespace FuryCore.Events;

using FuryCore.Models;

internal class MenuScrolled : SortedEventHandler<MenuScrolledEventArgs>
{
    public void Invoke(MenuScrolledEventArgs e)
    {
        this.InvokeAll(e);
    }
}