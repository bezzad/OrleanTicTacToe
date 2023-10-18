using GrainInterfaces.Models;
using Orleans;

namespace GrainInterfaces;

public interface IPairingGrain : IGrainWithIntegerKey
{
    Task AddGame(Guid gameId, string name, Guid owner);

    Task RemoveGame(Guid gameId);

    Task<PairingSummary> GetGame(Guid gameId);

    Task<PairingSummary[]> GetGames();
}