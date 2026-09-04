interface ScoreboardProps {
  xWins: number;
  oWins: number;
  draws: number;
}

export function Scoreboard({ xWins, oWins, draws }: ScoreboardProps) {
  const total = xWins + oWins + draws;

  return (
    <div className="scoreboard">
      <h3>Scoreboard</h3>
      <div className="score-grid">
        <div className="score-item">
          <span className="score-label">Player X</span>
          <span className="score-value">{xWins}</span>
        </div>
        <div className="score-item">
          <span className="score-label">Player O</span>
          <span className="score-value">{oWins}</span>
        </div>
        <div className="score-item">
          <span className="score-label">Draws</span>
          <span className="score-value">{draws}</span>
        </div>
        <div className="score-item total">
          <span className="score-label">Total</span>
          <span className="score-value">{total}</span>
        </div>
      </div>
    </div>
  );
}
