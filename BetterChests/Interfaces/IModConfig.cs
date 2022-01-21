namespace BetterChests.Interfaces;

public interface IModConfig
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="other"></param>
    /// <typeparam name="TOther"></typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IModConfig
    {
    }
}