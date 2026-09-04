namespace TicTacToe.Api.Models;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameState
{
    InProgress,
    PlayerXWon,
    PlayerOWon,
    Draw
}
