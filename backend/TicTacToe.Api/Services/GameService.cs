namespace TicTacToe.Api.Services;

using TicTacToe.Api.Models;

public interface IGameService
{
    GameSession CreateNewGame(GameMode mode = GameMode.TwoPlayer);
    GameResponse MakeMove(string gameId, int row, int column);
    GameResponse UndoLastMove(string gameId);
    GameResponse ResetGame(string gameId);
    GameResponse ResetScoreboard(string gameId);
    GameResponse GetGameState(string gameId);
}

public class GameService : IGameService
{
    private readonly Dictionary<string, GameSession> _games = new();

    public GameSession CreateNewGame(GameMode mode = GameMode.TwoPlayer)
    {
        var game = new GameSession(mode);
        _games[game.Id] = game;
        return game;
    }

    public GameResponse MakeMove(string gameId, int row, int column)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        // Validate move
        if (row < 0 || row > 2 || column < 0 || column > 2)
        {
            return new GameResponse { Message = "Invalid row or column. Must be 0-2." };
        }

        if (game.Board[row, column] != ' ')
        {
            return new GameResponse { Message = "Cell already occupied." };
        }

        if (game.State != GameState.InProgress)
        {
            return new GameResponse { Message = "Game is already finished." };
        }

        // Make the human move
        game.Board[row, column] = game.CurrentPlayer;

        var move = new Move
        {
            MoveNumber = game.MoveHistory.Count + 1,
            Player = game.CurrentPlayer,
            Row = row,
            Column = column,
            Timestamp = DateTime.UtcNow
        };

        game.MoveHistory.Add(move);

        // Check for human win
        if (CheckWin(game, row, column))
        {
            game.State = game.CurrentPlayer == 'X' ? GameState.PlayerXWon : GameState.PlayerOWon;
            if (game.CurrentPlayer == 'X')
            {
                game.XWins++;
            }
            else
            {
                game.OWins++;
            }

            return GetGameStateResponse(game);
        }
        // Check for draw
        else if (IsBoardFull(game.Board))
        {
            game.State = GameState.Draw;
            game.Draws++;

            return GetGameStateResponse(game);
        }
        else
        {
            // Switch player for next turn
            game.CurrentPlayer = game.CurrentPlayer == 'X' ? 'O' : 'X';

            // If in computer mode and it's now computer's turn, make computer move
            if (game.GameMode == GameMode.Computer && game.CurrentPlayer == 'O')
            {
                // Make computer move
                var computerMove = GetComputerMove(game);
                if (computerMove != null)
                {
                    // Apply computer move
                    game.Board[computerMove.Row, computerMove.Column] = game.CurrentPlayer;

                    var computerMoveRecord = new Move
                    {
                        MoveNumber = game.MoveHistory.Count + 1,
                        Player = game.CurrentPlayer,
                        Row = computerMove.Row,
                        Column = computerMove.Column,
                        Timestamp = DateTime.UtcNow
                    };

                    game.MoveHistory.Add(computerMoveRecord);

                    // Check for computer win
                    if (CheckWin(game, computerMove.Row, computerMove.Column))
                    {
                        game.State = GameState.PlayerOWon;
                        game.OWins++;
                    }
                    // Check for draw after computer move
                    else if (IsBoardFull(game.Board))
                    {
                        game.State = GameState.Draw;
                        game.Draws++;
                    }
                    else
                    {
                        // Switch back to human player
                        game.CurrentPlayer = 'X';
                    }
                }
            }
        }

        return GetGameStateResponse(game);
    }

    public GameResponse UndoLastMove(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        if (game.MoveHistory.Count == 0)
        {
            return new GameResponse { Message = "No moves to undo." };
        }

        // Remember if the game was completed before undoing
        bool wasCompleted = game.State != GameState.InProgress;
        GameState? wasWinState = null;
        if (wasCompleted)
        {
            if (game.State == GameState.PlayerXWon) wasWinState = GameState.PlayerXWon;
            else if (game.State == GameState.PlayerOWon) wasWinState = GameState.PlayerOWon;
        }

        // Determine how many moves to undo based on game mode
        int movesToUndo = game.GameMode == GameMode.Computer ? 2 : 1;
        // Ensure we don't try to undo more moves than exist
        movesToUndo = Math.Min(movesToUndo, game.MoveHistory.Count);

        // Remove the specified number of moves from board
        for (int i = 0; i < movesToUndo; i++)
        {
            if (game.MoveHistory.Count > 0)
            {
                var lastMove = game.MoveHistory[game.MoveHistory.Count - 1];
                game.Board[lastMove.Row, lastMove.Column] = ' ';
                game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
            }
        }

        // Evaluate the current board to determine the new state
        List<Position> winningPositions;
        bool hasWin = TryGetWinningPositions(game.Board, out winningPositions);
        GameState newState;
        if (hasWin)
        {
            // Determine the winner from the board (we can get from the first winning position)
            char winner = game.Board[winningPositions[0].Row, winningPositions[0].Column];
            newState = winner == 'X' ? GameState.PlayerXWon : GameState.PlayerOWon;
            game.WinningPositions = winningPositions;
        }
        else if (IsBoardFull(game.Board))
        {
            newState = GameState.Draw;
            game.WinningPositions.Clear();
        }
        else
        {
            newState = GameState.InProgress;
            game.WinningPositions.Clear();
        }

        game.State = newState;

        // Adjust scoreboard: remove the score for the previously completed game (if any)
        // and add the score for the newly completed game (if any)
        if (wasCompleted)
        {
            if (wasWinState == GameState.PlayerXWon) game.XWins--;
            else if (wasWinState == GameState.PlayerOWon) game.OWins--;
            else game.Draws--;
        }

        if (newState == GameState.PlayerXWon) game.XWins++;
        else if (newState == GameState.PlayerOWon) game.OWins++;
        else if (newState == GameState.Draw) game.Draws++;

        // Switch back to the player who should move next based on remaining moves
        if (game.MoveHistory.Count > 0)
        {
            var lastMove = game.MoveHistory[game.MoveHistory.Count - 1];
            game.CurrentPlayer = lastMove.Player == 'X' ? 'O' : 'X';
        }
        else
        {
            // No moves left, start with X
            game.CurrentPlayer = 'X';
        }

        return GetGameStateResponse(game);
    }

    /// <summary>
    /// Attempts to find a winning line on the board.
    /// </summary>
    private bool TryGetWinningPositions(char[,] board, out List<Position> winningPositions)
    {
        winningPositions = new List<Position>();
        // Check rows
        for (int row = 0; row < 3; row++)
        {
            if (board[row, 0] != ' ' && board[row, 0] == board[row, 1] && board[row, 1] == board[row, 2])
            {
                winningPositions.Add(new Position(row, 0));
                winningPositions.Add(new Position(row, 1));
                winningPositions.Add(new Position(row, 2));
                return true;
            }
        }
        // Check columns
        for (int col = 0; col < 3; col++)
        {
            if (board[0, col] != ' ' && board[0, col] == board[1, col] && board[1, col] == board[2, col])
            {
                winningPositions.Add(new Position(0, col));
                winningPositions.Add(new Position(1, col));
                winningPositions.Add(new Position(2, col));
                return true;
            }
        }
        // Check main diagonal
        if (board[0, 0] != ' ' && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
        {
            winningPositions.Add(new Position(0, 0));
            winningPositions.Add(new Position(1, 1));
            winningPositions.Add(new Position(2, 2));
            return true;
        }
        // Check anti-diagonal
        if (board[0, 2] != ' ' && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
        {
            winningPositions.Add(new Position(0, 2));
            winningPositions.Add(new Position(1, 1));
            winningPositions.Add(new Position(2, 0));
            return true;
        }
        return false;
    }

    public GameResponse ResetGame(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        // Only reset scoreboard, preserve game state
        return GetGameStateResponse(game);
    }

    public GameResponse ResetScoreboard(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        // Only reset scoreboard, preserve game state
        game.XWins = 0;
        game.OWins = 0;
        game.Draws = 0;

        return GetGameStateResponse(game);
    }

    public GameResponse GetGameState(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        return GetGameStateResponse(game);
    }

    private bool CheckWin(GameSession game, int lastRow, int lastColumn)
    {
        var player = game.Board[lastRow, lastColumn];
        game.WinningPositions.Clear();

        // Check row
        if (CheckLine(game.Board, (lastRow, 0), (lastRow, 1), (lastRow, 2), player))
        {
            game.WinningPositions = new List<Position> 
            { 
                new Position(lastRow, 0), 
                new Position(lastRow, 1), 
                new Position(lastRow, 2) 
            };
            return true;
        }

        // Check column
        if (CheckLine(game.Board, (0, lastColumn), (1, lastColumn), (2, lastColumn), player))
        {
            game.WinningPositions = new List<Position> 
            { 
                new Position(0, lastColumn), 
                new Position(1, lastColumn), 
                new Position(2, lastColumn) 
            };
            return true;
        }

        // Check main diagonal
        if (lastRow == lastColumn && CheckLine(game.Board, (0, 0), (1, 1), (2, 2), player))
        {
            game.WinningPositions = new List<Position> 
            { 
                new Position(0, 0), 
                new Position(1, 1), 
                new Position(2, 2) 
            };
            return true;
        }

        // Check anti-diagonal
        if (lastRow + lastColumn == 2 && CheckLine(game.Board, (0, 2), (1, 1), (2, 0), player))
        {
            game.WinningPositions = new List<Position> 
            { 
                new Position(0, 2), 
                new Position(1, 1), 
                new Position(2, 0) 
            };
            return true;
        }

        return false;
    }

    private bool CheckLine(char[,] board, (int, int) pos1, (int, int) pos2, (int, int) pos3, char player)
    {
        return board[pos1.Item1, pos1.Item2] == player &&
               board[pos2.Item1, pos2.Item2] == player &&
               board[pos3.Item1, pos3.Item2] == player;
    }

    /// <summary>
    /// Determines if the specified player can win by placing a mark at the given position
    /// </summary>
    private bool WouldWin(GameSession game, int row, int column, char player)
    {
        // Temporarily place the mark
        char original = game.Board[row, column];
        game.Board[row, column] = player;

        // Check if this creates a win
        bool wins = CheckWin(game, row, column);

        // Restore the original state
        game.Board[row, column] = original;

        return wins;
    }

    /// <summary>
    /// Gets the best move for the computer based on the priority algorithm
    /// </summary>
    private Position? GetComputerMove(GameSession game)
    {
        // Priority 1: If O can win, play the winning move
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (game.Board[row, col] == ' ' && WouldWin(game, row, col, 'O'))
                {
                    return new Position(row, col);
                }
            }
        }

        // Priority 2: If X can win next, block X
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (game.Board[row, col] == ' ' && WouldWin(game, row, col, 'X'))
                {
                    return new Position(row, col);
                }
            }
        }

        // Priority 3: Take center if available
        if (game.Board[1, 1] == ' ')
        {
            return new Position(1, 1);
        }

        // Priority 4: Take a corner if available
        var corners = new[] {
            new Position(0, 0), new Position(0, 2),
            new Position(2, 0), new Position(2, 2)
        };

        foreach (var corner in corners)
        {
            if (game.Board[corner.Row, corner.Column] == ' ')
            {
                return corner;
            }
        }

        // Priority 5: Take any available cell
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (game.Board[row, col] == ' ')
                {
                    return new Position(row, col);
                }
            }
        }

        // Should never reach here if called when moves are available
        return null;
    }

    private bool IsBoardFull(char[,] board)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[i, j] == ' ')
                    return false;
            }
        }
        return true;
    }

    private GameResponse GetGameStateResponse(GameSession game)
    {
        var response = new GameResponse
        {
            GameId = game.Id,
            Board = ConvertBoardToStringJagged(game.Board),
            CurrentPlayer = game.CurrentPlayer.ToString(),
            State = game.State,
            MoveHistory = game.MoveHistory,
            WinningPositions = game.WinningPositions,
            XWins = game.XWins,
            OWins = game.OWins,
            Draws = game.Draws
        };

        return response;
    }

    private char[][] ConvertBoardToJagged(char[,] board)
    {
        var jagged = new char[3][];
        for (int i = 0; i < 3; i++)
        {
            jagged[i] = new char[3];
            for (int j = 0; j < 3; j++)
            {
                jagged[i][j] = board[i, j];
            }
        }
        return jagged;
    }

    private string[][] ConvertBoardToStringJagged(char[,] board)
    {
        var jagged = new string[3][];
        for (int i = 0; i < 3; i++)
        {
            jagged[i] = new string[3];
            for (int j = 0; j < 3; j++)
            {
                jagged[i][j] = board[i, j].ToString();
            }
        }
        return jagged;
    }
}
