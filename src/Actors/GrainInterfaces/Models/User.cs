﻿namespace GrainInterfaces.Models;

[GenerateSerializer]
public class User
{
    [Id(0)]
    public Guid Id { get; set; }
    [Id(1)]
    public int Wins { get; set; }
    [Id(2)]
    public int Loses { get; set; }
    [Id(3)]
    public int GamesStarted { get; set; }
    [Id(4)]
    public string Username { get; set; }
    [Id(5)]
    public string Email { get; set; }
    [Id(6)]
    public string ClientConnectionId { get; set; }
    [Id(7)]
    public List<Guid> ActiveGames { get; set; }
    [Id(8)]
    public List<Guid> PastGames { get; set; }
}
