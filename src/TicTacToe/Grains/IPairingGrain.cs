using TicTacToe.Models;

namespace TicTacToe.Grains;

public interface IPairingGrain : IGrainWithIntegerKey
{
    Task AddGame(Guid gameId, string name, Guid owner);

    Task RemoveGame(Guid gameId);

    Task<PairingSummary> GetGame(Guid gameId);

    Task<PairingSummary[]> GetGames();
}