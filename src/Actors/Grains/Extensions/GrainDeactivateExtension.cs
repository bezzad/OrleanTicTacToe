using GrainInterfaces.Extensions;
using Orleans.Runtime;

namespace Grains.Extensions;

public sealed class GrainDeactivateExtension : IGrainDeactivateExtension
{
    private IGrainContext _context;

    public GrainDeactivateExtension(IGrainContext context)
    {
        _context = context;
    }

    public async Task Deactivate(string msg)
    {
        var reason = new DeactivationReason(DeactivationReasonCode.ApplicationRequested, msg);
        await _context.DeactivateAsync(reason);
    }
}
