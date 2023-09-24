using Orleans.Concurrency;
using System.Collections.Concurrent;

namespace TicTacToe.Grains;

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

    public Task<PairingSummary[]> GetGames() => Task.FromResult(_cache.Values.ToArray());
}
