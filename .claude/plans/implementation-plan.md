# Implementation Plan: Adding Computer Mode and Related Features

## Context
The current TicTacToe implementation has a working two-player mode but is missing several key features specified in the requirements:
1. Computer mode (Play Against Computer)
2. Game mode selection (Two Player vs Computer)
3. Computer move logic with priority algorithm
4. Proper undo behavior for computer mode (removing computer+human pair)
5. Game mode tracking and UI indication

Based on the analysis in `current-status-analysis.md`, here's the detailed implementation plan.

## Implementation Approach

### Phase 1: Backend Changes - Add Game Mode Support

#### 1.1 Update GameSession Model
**File:** `D:\git\TicTacToe\backend\TicTacToe.Api\Models\GameSession.cs`
- Add `GameMode` property with enum
- Initialize with default value (TwoPlayer)
- Update constructor to accept game mode parameter

#### 1.2 Create GameMode Enum
**File:** `D:\git\TicTacToe\backend\TicTacToe.Api\Models\GameMode.cs` (new file)
```csharp
namespace TicTacToe.Api.Models;
public enum GameMode
{
    TwoPlayer,
    Computer
}
```

#### 1.3 Update GameService for Computer Logic
**File:** `D:\git\TicTacToe\backend\TicTacToe.Api\Services\GameService.cs`
- Add computer move calculation method implementing priority algorithm:
  1. If O can win, play winning move
  2. If X can win next, block X
  3. Take center if available
  4. Take a corner if available
  5. Take any available cell
- Modify MakeMove to handle computer mode:
  - After human move, if in computer mode and game not finished, automatically make computer move
- Update UndoLastMove to handle game mode differences:
  - TwoPlayer mode: remove 1 move (current behavior)
  - Computer mode: remove 2 moves (computer+human pair)
- Update GetGameStateResponse to include game mode

#### 1.4 Update GameController
**File:** `D:\git\TicTacToe\backend\TicTacToe.Api\Controllers\GameController.cs`
- Modify CreateNewGame to accept game mode parameter (optional, default TwoPlayer)
- Or add new endpoint: POST /api/games/mode/{mode}
- Update existing endpoints to work with game mode

### Phase 2: Frontend Changes - Add UI and Integration

#### 2.1 Add GameMode Types
**File:** `D:\git\TicTacToe\frontend\src\types.ts`
- Add GameMode enum matching backend
- Update GameResponse to include gameMode property

#### 2.2 Update Game Status Component
**File:** `D:\git\TicTacToe\frontend\src\components\GameStatus.tsx`
- Add game mode selector (radio buttons or toggle)
- Display current game mode
- Pass game mode to API calls
- Handle mode change (requires new game)

#### 2.3 Update Main App Logic
**File:** `D:\git\TicTacToe\frontend\src\main.tsx`
- Add game mode state
- When creating new game, include game mode
- After human move in computer mode, automatically trigger computer move
- Update canUndo logic based on game mode and move history

#### 2.4 Update API Service
**File:** `D:\git\TicTacToe\frontend\src\services\gameApiService.ts`
- Add gameMode parameter to createNewGame
- Update move-making functions to include game mode if needed

### Phase 3: Detailed Implementation Steps

#### Backend Implementation Details:

**Step 1: Add GameMode Enum**
Create `GameMode.cs` in Models folder with TwoPlayer and Computer values.

**Step 2: Update GameSession**
Add GameMode property:
```csharp
public GameMode GameMode { get; set; } = GameMode.TwoPlayer;
```

Update constructor to accept optional gameMode parameter.

**Step 3: Implement Computer Move Logic**
Add method to GameService:
```csharp
private Position GetComputerMove(GameSession game)
{
    // Priority 1: Check if O can win
    var winMove = GetWinningMove(game, 'O');
    if (winMove != null) return winMove;
    
    // Priority 2: Block X if they can win next
    var blockMove = GetWinningMove(game, 'X');
    if (blockMove != null) return blockMove;
    
    // Priority 3: Take center
    if (game.Board[1, 1] == ' ') return new Position(1, 1);
    
    // Priority 4: Take a corner
    var corners = new[] { 
        new Position(0, 0), new Position(0, 2),
        new Position(2, 0), new Position(2, 2)
    };
    foreach (var corner in corners)
    {
        if (game.Board[corner.Row, corner.Column] == ' ')
            return corner;
    }
    
    // Priority 5: Take any available cell
    for (int row = 0; row < 3; row++)
    {
        for (int col = 0; col < 3; col++)
        {
            if (game.Board[row, col] == ' ')
                return new Position(row, col);
        }
    }
    
    return null; // Should never happen if called when moves available
}

private Position? GetWinningMove(GameSession game, char player)
{
    // Check all empty cells to see if placing player there would win
    for (int row = 0; row < 3; row++)
    {
        for (int col = 0; col < 3; col++)
        {
            if (game.Board[row, col] == ' ')
            {
                // Try the move
                game.Board[row, col] = player;
                bool wouldWin = CheckWin(game, row, col);
                // Undo the trial move
                game.Board[row, col] = ' ';
                
                if (wouldWin)
                    return new Position(row, col);
            }
        }
    }
    return null;
}
```

**Step 4: Modify MakeMove for Computer Mode**
After human move processing:
```csharp
// After checking for win/draw and before switching player
if (game.State == GameState.InProgress && 
    game.GameMode == GameMode.Computer && 
    game.CurrentPlayer == 'O') // Computer's turn
{
    // Make computer move
    var computerMove = GetComputerMove(game);
    if (computerMove != null)
    {
        // Apply computer move (similar to human move logic)
        game.Board[computerMove.Row, computerMove.Column] = game.CurrentPlayer;
        
        var computerMoveRecord = new Move
        {
            MoveNumber = game.MoveHistory.Count + 1,
            Player = game.CurrentPlayer.ToString(),
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
```

**Step 5: Update UndoLastMove**
```csharp
public GameResponse UndoLastMove(string gameId)
{
    // ... existing validation ...
    
    int movesToRemove = game.GameMode == GameMode.Computer ? 2 : 1;
    // Ensure we don't try to remove more moves than exist
    movesToRemove = Math.Min(movesToRemove, game.MoveHistory.Count);
    
    for (int i = 0; i < movesToRemove; i++)
    {
        if (game.MoveHistory.Count > 0)
        {
            var lastMove = game.MoveHistory[game.MoveHistory.Count - 1];
            game.Board[lastMove.Row, lastMove.Column] = ' ';
            game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
        }
    }
    
    // Reset game state and recalculate
    game.State = GameState.InProgress;
    game.WinningPositions.Clear();
    
    // Set current player to the player who would move next
    if (game.MoveHistory.Count > 0)
    {
        var lastMove = game.MoveHistory[game.MoveHistory.Count - 1];
        game.CurrentPlayer = lastMove.Player == "X" ? 'O' : 'X';
    }
    else
    {
        game.CurrentPlayer = 'X'; // No moves, start with X
    }
    
    // Recalculate win/draw status based on remaining moves
    if (game.MoveHistory.Count >= 3) // Need at least 3 moves for a win
    {
        // Check if current state is win/draw
        var lastMove = game.MoveHistory[game.MoveHistory.Count - 1];
        if (CheckWin(game, lastMove.Row, lastMove.Column))
        {
            game.State = lastMove.Player == "X" ? GameState.PlayerXWon : GameState.PlayerOWon;
            // Update scoreboard accordingly
            if (lastMove.Player == "X") game.XWins++;
            else game.OWins++;
        }
        else if (IsBoardFull(game.Board))
        {
            game.State = GameState.Draw;
            game.Draws++;
        }
    }
    
    return GetGameStateResponse(game);
}
```

**Step 6: Update GameController**
Modify CreateNewGame endpoint:
```csharp
[HttpPost("new")]
public ActionResult<GameResponse> CreateNewGame([FromQuery] GameMode? mode = null)
{
    var game = _gameService.CreateNewGame(mode ?? GameMode.TwoPlayer);
    var response = _gameService.GetGameState(game.Id);
    return Ok(response);
}
```

#### Frontend Implementation Details:

**Step 1: Add GameMode to types.ts**
```typescript
export enum GameMode {
  TwoPlayer = "TwoPlayer",
  Computer = "Computer"
}

export interface GameResponse {
  // ... existing fields ...
  gameMode: GameMode;
}
```

**Step 2: Update GameStatus Component**
Add game mode selector:
```typescript
interface GameStatusProps {
  // ... existing props ...
  gameMode: GameMode;
  onGameModeChange: (mode: GameMode) => void;
}

// In JSX:
<div className="mode-selector">
  <label>
    <input
      type="radio"
      value={GameMode.TwoPlayer}
      checked={gameMode === GameMode.TwoPlayer}
      onChange={(e) => onGameModeChange(GameMode.TwoPlayer as GameMode)}
    />
    Two Player
  </label>
  <label>
    <input
      type="radio"
      value={GameMode.Computer}
      checked={gameMode === GameMode.Computer}
      onChange={(e) => onGameModeChange(GameMode.Computer as GameMode)}
    />
    Play Against Computer
  </label>
</div>
```

**Step 3: Update main.tsx**
Add game mode state and logic:
```typescript
const [gameMode, setGameMode] = useState<GameMode>(GameMode.TwoPlayer);

// When creating new game:
const response = await gameApiService.createNewGame(gameMode);

// After human move:
const response = await gameApiService.makeMove(gameData.gameId, row, column);
setGameData(response);

// If in computer mode and game not finished and it's computer's turn,
// trigger computer move after a short delay
if (gameData?.gameMode === GameMode.Computer && 
    gameData.state === GameState.InProgress && 
    gameData.currentPlayer === 'O') {
  setTimeout(async () => {
    try {
      const computerResponse = await gameApiService.makeMove(
        gameData.gameId, 
        -1, // -1 indicates computer move (handle in backend)
        -1
      );
      setGameData(computerResponse);
    } catch (err) {
      console.error('Computer move failed:', err);
    }
  }, 500); // Small delay for better UX
}

// Update canUndo logic:
const canUndo = gameData?.moveHistory.length > 0 && 
                !(gameData.gameMode === GameMode.Computer && 
                  gameData.moveHistory.length === 1);
```

**Step 4: Update API Service**
Modify createNewGame to accept gameMode:
```typescript
createNewGame: (gameMode: GameMode = GameMode.TwoPlayer): Promise<GameResponse> => {
  return api.get(`/api/games/new?mode=${gameMode}`);
}
```

### Phase 4: Testing Strategy

#### Backend Tests to Add:
1. Computer move priority algorithm tests
2. Undo behavior in computer mode (removes 2 moves)
3. Game mode persistence through game lifecycle
4. Scoreboard updates in computer mode
5. Edge cases: computer move when board full, game already won

#### Frontend Tests to Add:
1. Game mode selector renders correctly
2. Game mode persists through game reset
3. Computer move triggers automatically after human move
4. Undo button disabled appropriately in computer mode
5. UI shows current game mode

#### Manual Testing Checklist:
☐ Create new game in Two Player mode - verify existing functionality
☐ Create new game in Computer mode - verify UI shows correct mode
☐ In Computer mode:
   - Human makes move
   - Computer automatically responds
   - Computer follows priority algorithm
   - Win/draw detection works
   - Scoreboard updates
☐ Undo in Two Player mode: removes one move
☐ Undo in Computer mode: removes computer+human pair
☐ Reset game works in both modes
☐ Reset scoreboard works in both modes
☐ Invalid moves are handled correctly in both modes

## Files to Modify:

### Backend:
1. `D:\git\TicTacToe\backend\TicTacToe.Api\Models\GameMode.cs` (new)
2. `D:\git\TicTacToe\backend\TicTacToe.Api\Models\GameSession.cs`
3. `D:\git\TicTacToe\backend\TicTacToe.Api\Models\MakeMoveRequest.cs` (if needed)
4. `D:\git\TicTacToe\backend\TicTacToe.Api\Services\GameService.cs`
5. `D:\git\TicTacToe\backend\TicTacToe.Api\Controllers\GameController.cs`

### Frontend:
1. `D:\git\TicTacToe\frontend\src\types.ts`
2. `D:\git\TicTacToe\frontend\src\components\GameStatus.tsx`
3. `D:\git\TicTacToe\frontend\src\main.tsx`
4. `D:\git\TicTacToe\frontend\src\services\gameApiService.ts`

## Assumptions:
1. Computer always plays as O, human as X (per requirements)
2. Game mode is set at game creation and cannot be changed mid-game
3. In computer mode, human always moves first
4. Game state is stored in-memory (no persistence required)
5. Standard 3x3 board size
6. Backend runs on localhost:5118 (as seen in frontend code)

## Trade-offs:
1. **Computer move calculation**: Implemented in backend for consistency with requirements stating "backend should own game session state"
2. **Mode change**: Requires creating new game rather than switching mid-game to simplify state management
3. **Computer move triggering**: Uses setTimeout in frontend for better UX, but actual logic resides in backend
4. **Undo logic**: Modified to handle both modes in a single method with conditional logic

## Verification Plan:
1. Unit tests for computer move algorithm
2. Unit tests for undo behavior in both modes
3. Integration tests for game mode flow
4. Manual verification of all acceptance criteria
5. Testing edge cases and error conditions

## Estimated Effort:
- Backend changes: 2-3 hours
- Frontend changes: 2-3 hours
- Testing: 1-2 hours
- Total: 5-8 hours