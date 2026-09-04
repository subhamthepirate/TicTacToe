import { StrictMode, useEffect, useState } from 'react'
import { createRoot } from 'react-dom/client'
import './styles.css'
import { GameBoard } from './components/GameBoard'
import { GameStatus } from './components/GameStatus'
import { MoveHistory } from './components/MoveHistory'
import { Scoreboard } from './components/Scoreboard'
import { ErrorBoundary } from './components/ErrorBoundary'
import gameApiService from './services/gameApiService'
import { GameResponse, GameState } from './types'

function App() {
  const [gameData, setGameData] = useState<GameResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Initialize game on component mount
  useEffect(() => {
    const initializeGame = async () => {
      try {
        setLoading(true)
        const response = await gameApiService.createNewGame()
        setGameData(response)
        setError(null)
      } catch (err) {
        setError('Failed to initialize game. Make sure the backend is running on http://localhost:5118')
        console.error(err)
      } finally {
        setLoading(false)
      }
    }

    initializeGame()
  }, [])

  const handleCellClick = async (row: number, column: number) => {
    if (!gameData) return

    try {
      const response = await gameApiService.makeMove(gameData.gameId, row, column)
      setGameData(response)
    } catch (err) {
      setError('Failed to make move')
      console.error(err)
    }
  }

  const handleUndoMove = async () => {
    if (!gameData) return

    try {
      const response = await gameApiService.undoLastMove(gameData.gameId)
      setGameData(response)
    } catch (err) {
      setError('Failed to undo move')
      console.error(err)
    }
  }

  const handleResetGame = async () => {
    if (!gameData) return

    try {
      const response = await gameApiService.resetGame(gameData.gameId)
      setGameData(response)
    } catch (err) {
      setError('Failed to reset game')
      console.error(err)
    }
  }

  const handleResetScoreboard = async () => {
    if (!gameData) return

    try {
      const response = await gameApiService.resetScoreboard(gameData.gameId)
      setGameData(response)
    } catch (err) {
      setError('Failed to reset scoreboard')
      console.error(err)
    }
  }

  if (loading) {
    return (
      <main className="app-container">
        <h1>Tic-Tac-Toe</h1>
        <div className="loading">Loading game...</div>
      </main>
    )
  }

  if (error) {
    return (
      <main className="app-container">
        <h1>Tic-Tac-Toe</h1>
        <div className="error">{error}</div>
        <button onClick={() => window.location.reload()}>Retry</button>
      </main>
    )
  }

  if (!gameData) {
    return (
      <main className="app-container">
        <h1>Tic-Tac-Toe</h1>
        <div className="error">Failed to load game</div>
      </main>
    )
  }

  const isGameFinished = gameData.state !== GameState.InProgress

  return (
    <main className="app-container">
      <h1>🎮 Tic-Tac-Toe</h1>
      
      <div className="game-container">
        <div className="main-section">
          <GameStatus
            currentPlayer={gameData.currentPlayer}
            gameState={gameData.state}
            onResetGame={handleResetGame}
            onResetScoreboard={handleResetScoreboard}
            onUndoMove={handleUndoMove}
            canUndo={gameData.moveHistory.length > 0}
          />

          <GameBoard
            board={gameData.board}
            onCellClick={handleCellClick}
            winningPositions={gameData.winningPositions}
            gameFinished={isGameFinished}
          />
        </div>

        <div className="side-section">
          <Scoreboard
            xWins={gameData.xWins}
            oWins={gameData.oWins}
            draws={gameData.draws}
          />

          <MoveHistory moves={gameData.moveHistory} />
        </div>
      </div>
    </main>
  )
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorBoundary>
      <App />
    </ErrorBoundary>
  </StrictMode>,
)