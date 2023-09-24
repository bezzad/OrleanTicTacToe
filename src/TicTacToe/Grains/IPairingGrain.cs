namespace TicTacToe.Grains;

public interface IPairingGrain : IGrainWithIntegerKey
{
    Task AddGame(Guid gameId, string name, Guid owner);

    Task RemoveGame(Guid gameId);

    Task<PairingSummary[]> GetGames();
}

[Immutable]
[GenerateSerializer]
public class PairingSummary
{
    [Id(0)] public Guid GameId { get; set; }
    [Id(1)] public string? Name { get; set; }
    [Id(2)] public Guid OwnerPlayerId { get; set; }
}
