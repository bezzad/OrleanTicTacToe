namespace TicTacToe.Models;

[GenerateSerializer]
public struct GameSummary
{
    [Id(0)] public GameState State { get; set; }
    [Id(1)] public bool YourMove { get; set; }
    [Id(2)] public int NumMoves { get; set; }
    [Id(3)] public GameOutcome Outcome { get; set; }
    [Id(4)] public int NumPlayers { get; set; }
    [Id(5)] public Guid GameId { get; set; }
    [Id(6)] public string[] Usernames { get; set; }
    [Id(7)] public string Name { get; set; }
    [Id(8)] public bool GameStarter { get; set; }
}
