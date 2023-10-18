using GrainInterfaces;
using GrainInterfaces.Models;
using Orleans;
using Orleans.Concurrency;
using System.Collections.Concurrent;

namespace Grains;

[Reentrant]
public class PairingGrain : Grain, IPairingGrain
{
    //  System.Runtime.Caching.MemoryCache _cache = new("pairing");
    private static readonly ConcurrentDictionary<Guid, PairingSummary> _cache = new();

    public Task AddGame(Guid gameId, string name, Guid owner)
    {
        _cache.TryAdd(gameId, new PairingSummary { GameId = gameId, Name = name, OwnerPlayerId = owner });
        return Task.CompletedTask;
    }

    public Task RemoveGame(Guid gameId)
    {
        _cache.TryRemove(gameId, out var _);
        return Task.CompletedTask;
    }

    public Task<PairingSummary> GetGame(Guid gameId)
    {
        if (_cache.TryGetValue(gameId, out var value))
        {
            return Task.FromResult(value);
        }

        return null;
    }

    public Task<PairingSummary[]> GetGames() => Task.FromResult(_cache.Values.ToArray());
}
