namespace TicTacToe.Models;

[GenerateSerializer]
public struct GameMove
{
    [Id(0)] public Guid PlayerId { get; set; }
    [Id(1)] public int X { get; set; }
    [Id(2)] public int Y { get; set; }
}
