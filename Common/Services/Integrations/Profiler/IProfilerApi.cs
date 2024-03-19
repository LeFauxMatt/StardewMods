namespace StardewMods.Common.Services.Integrations.Profiler;

using System.Reflection;

public interface IProfilerApi
{
    public IDisposable RecordSection(string ModId, string EventType, string Details);

    public MethodBase AddGenericDurationPatch(string type, string method, string detailsType = null!);
}