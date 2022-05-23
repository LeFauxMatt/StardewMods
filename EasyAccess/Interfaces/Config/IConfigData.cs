#nullable disable

namespace StardewMods.EasyAccess.Interfaces.Config;

using StardewMods.EasyAccess.Models.Config;

/// <summary>
///     Mod config data.
/// </summary>
internal interface IConfigData
{
    /// <summary>
    ///     Gets or sets a value indicating whether Configurator will be enabled.
    /// </summary>
    bool Configurator { get; set; }

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    ControlScheme ControlScheme { get; set; }

    /// <summary>
    ///     Gets or sets the default producer configuration.
    /// </summary>
    ProducerData DefaultProducer { get; set; }

    /// <summary>
    ///     Copies data from one <see cref="IConfigData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IConfigData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IConfigData" />.</typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IConfigData
    {
        other.Configurator = this.Configurator;
        ((IControlScheme)other.ControlScheme).CopyTo(this.ControlScheme);
        ((IProducerData)other.DefaultProducer).CopyTo(this.DefaultProducer);
    }
}