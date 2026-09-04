import { Move } from "../types";

interface MoveHistoryProps {
  moves: Move[];
}

export function MoveHistory({ moves }: MoveHistoryProps) {
  return (
    <div className="move-history">
      <h3>Move History</h3>
      {moves.length === 0 ? (
        <p className="empty-message">No moves yet</p>
      ) : (
        <table className="moves-table">
          <thead>
            <tr>
              <th>Move</th>
              <th>Player</th>
              <th>Position</th>
            </tr>
          </thead>
          <tbody>
            {moves.map((move) => (
              <tr key={move.moveNumber}>
                <td>{move.moveNumber}</td>
                <td>{move.player}</td>
                <td>
                  Row {move.row + 1}, Column {move.column + 1}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
