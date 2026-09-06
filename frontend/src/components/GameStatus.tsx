import { GameState, GameMode } from "../types";

interface GameStatusProps {
  currentPlayer: string;
  gameState: GameState;
  gameMode: GameMode;
  onResetGame: () => void;
  onResetScoreboard: () => void;
  onUndoMove: () => void;
  onGameModeChange: (mode: GameMode) => void;
  canUndo: boolean;
}

export function GameStatus({
  currentPlayer,
  gameState,
  gameMode,
  onResetGame,
  onResetScoreboard,
  onUndoMove,
  onGameModeChange,
  canUndo,
}: GameStatusProps) {
  const getStatusMessage = () => {
    switch (gameState) {
      case GameState.InProgress:
        return `Current Player: ${currentPlayer}`;
      case GameState.PlayerXWon:
        return "🎉 Player X Won!";
      case GameState.PlayerOWon:
        return "🎉 Player O Won!";
      case GameState.Draw:
        return "🤝 It's a Draw!";
      default:
        return "";
    }
  };

  const isGameFinished = gameState !== GameState.InProgress;

  return (
    <div className="game-status">
      <div className="mode-selector">
        <label>
          <input
            type="radio"
            value={GameMode.TwoPlayer}
            checked={gameMode === GameMode.TwoPlayer}
            onChange={(e) => onGameModeChange(GameMode.TwoPlayer)}
          />
          Two Player
        </label>
        <label>
          <input
            type="radio"
            value={GameMode.Computer}
            checked={gameMode === GameMode.Computer}
            onChange={(e) => onGameModeChange(GameMode.Computer)}
          />
          Play Against Computer
        </label>
      </div>
      <h2>{getStatusMessage()}</h2>
      <div className="button-group">
        <button
          onClick={onUndoMove}
          disabled={!canUndo}
          className="btn btn-secondary"
        >
          ↶ Undo Move
        </button>
        <button onClick={onResetGame} className="btn btn-primary">
          🔄 Reset Game
        </button>
        <button onClick={onResetScoreboard} className="btn btn-danger">
          🗑️ Reset Scoreboard
        </button>
      </div>
    </div>
  );
}
