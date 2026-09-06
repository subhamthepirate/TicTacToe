namespace TicTacToe.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost("new")]
    public ActionResult<GameResponse> CreateNewGame([FromQuery] GameMode? mode = null)
    {
        var game = _gameService.CreateNewGame(mode ?? GameMode.TwoPlayer);
        var response = _gameService.GetGameState(game.Id);
        return Ok(response);
    }

    [HttpPost("{gameId}/move")]
    public ActionResult<GameResponse> MakeMove(string gameId, [FromBody] MakeMoveRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = _gameService.MakeMove(gameId, request.Row, request.Column);
        
        if (response.Message.Contains("not found"))
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPost("{gameId}/undo")]
    public ActionResult<GameResponse> UndoLastMove(string gameId)
    {
        var response = _gameService.UndoLastMove(gameId);

        if (response.Message.Contains("not found"))
        {
            return NotFound(response);
        }

        if (response.Message.Contains("No moves"))
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{gameId}/reset")]
    public ActionResult<GameResponse> ResetGame(string gameId)
    {
        var response = _gameService.ResetGame(gameId);

        if (response.Message.Contains("not found"))
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPost("{gameId}/reset-scoreboard")]
    public ActionResult<GameResponse> ResetScoreboard(string gameId)
    {
        var response = _gameService.ResetScoreboard(gameId);

        if (response.Message.Contains("not found"))
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpGet("{gameId}")]
    public ActionResult<GameResponse> GetGameState(string gameId)
    {
        var response = _gameService.GetGameState(gameId);

        if (response.Message.Contains("not found"))
        {
            return NotFound(response);
        }

        return Ok(response);
    }
}
