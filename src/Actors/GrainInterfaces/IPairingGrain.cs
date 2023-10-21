using GrainInterfaces.Models;
using Orleans.Concurrency;

namespace GrainInterfaces;

public interface IPairingGrain : IGrainWithIntegerKey
{
    Task AddGame(Guid gameId, string name, Guid owner);

    Task RemoveGame(Guid gameId);

    [AlwaysInterleave]
    [ReadOnly]
    Task<PairingSummary> GetGame(Guid gameId);

    [AlwaysInterleave]
    [ReadOnly]
    Task<PairingSummary[]> GetGames();
}