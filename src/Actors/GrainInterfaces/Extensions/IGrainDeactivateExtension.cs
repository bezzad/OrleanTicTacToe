using Orleans.Runtime;

namespace GrainInterfaces.Extensions;

public interface IGrainDeactivateExtension : IGrainExtension
{
    Task Deactivate(string msg);
}
