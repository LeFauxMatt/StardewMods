namespace FuryCore.Interfaces;

/// <summary>
///     An <see cref="IService" /> which has not been instantiated yet.
/// </summary>
internal interface IPendingService
{
    /// <summary>
    ///     Forces the Lazy service value to be evaluated.
    /// </summary>
    public void ForceEvaluation();
}