namespace FuryCore;

using System.Linq;
using System.Reflection;
using FuryCore.Attributes;
using FuryCore.Services;

/// <inheritdoc />
public class FuryCoreApi : ServiceCollection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FuryCoreApi"/> class.
    /// </summary>
    /// <param name="services"></param>
    public FuryCoreApi(ServiceCollection services)
    {
        foreach (var service in services.Where(service => service.GetType().GetCustomAttribute<FuryCoreServiceAttribute>()?.Exportable == true))
        {
            this.Add(service);
        }
    }
}