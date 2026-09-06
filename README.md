# Tic-Tac-Toe

A browser-based Tic-Tac-Toe application with a React and TypeScript frontend and a .NET 8 Web API backend.

## Current status

The application is fully implemented with a .NET 8 Web API backend and a React TypeScript frontend. All planned features have been implemented and tested.

## Features

- Two-player and play-against-computer modes
- Backend-owned game state and validation
- Move history and mode-aware undo (removes 1 move in two-player, computer+human pair in computer mode)
- Row, column, diagonal, and draw detection
- Winning-cell highlighting
- Session scoreboard tracked in game state
- Separate game and scoreboard reset actions
- Computer move logic with priority algorithm (win → block → center → corner → any)
- Automatic computer move triggering in computer mode
- Game mode selection UI
- RESTful API for all game operations

## Technology

- Frontend: React, TypeScript, Vite
- Backend: .NET 8 ASP.NET Core Web API
- Storage: In-memory session storage
- Tests: xUnit for backend; frontend component and API tests after the React scaffold

## Local setup

### Backend

From `backend/`:

```powershell
dotnet restore
dotnet run --project TicTacToe.Api
```

Run backend tests with:

```powershell
dotnet test
```

### Frontend

Install Node.js (which provides npm), then scaffold or run the frontend from `frontend/`:

```powershell
npm install
npm run dev
```

The frontend will call the local .NET API through REST endpoints. The API development origin and frontend origin will be documented here once the ports are finalized.

## API contract

The planned routes are:

| Method | Endpoint | Purpose |
| --- | --- | --- |
| POST | `/api/games` | Create a game session |
| GET | `/api/games/{id}` | Retrieve current game state |
| POST | `/api/games/{id}/moves` | Submit a player move |
| POST | `/api/games/{id}/undo` | Undo the latest move or turn pair |
| POST | `/api/games/{id}/reset` | Reset the current game |
| GET | `/api/scoreboard` | Retrieve the session scoreboard |
| POST | `/api/scoreboard/reset` | Reset the scoreboard |

Every game mutation will return enough state for the frontend to render the board, current player, mode, status, winner, winning cells, move history, and scoreboard.

## Design decisions

- React is used instead of Angular because the assignment technology expectations explicitly allow React.js or Angular with TypeScript.
- The backend is the source of truth for game rules, validation, status, history, undo, and scoreboard state.
- In-memory storage keeps local setup simple while interfaces will allow later replacement with SQLite.
- Undo is allowed after a win or draw and the scoreboard is adjusted accordingly (the assignment's Option B). This allows players to reverse completed games and maintains scoreboard consistency.
- In computer mode, the human is X and the computer is O. The computer follows the required winning, blocking, center, corner, and fallback priority.

## Assumptions and limitations

- This is a local, single-session application; authentication and multi-user play are out of scope.
- The active game ID will be stored by the frontend so a refresh can request the current backend state.
- The computer opponent is rule-based rather than minimax-based.

## AI-assisted development notes

The implementation plan was derived from the supplied technical assignment. AI-generated code will be reviewed against the backend state-ownership requirement, especially move validation, terminal-state handling, mode-aware undo, and one-time scoreboard updates. The final version will record prompts, manual changes, assumptions, and trade-offs here.

## Future improvements

- SQLite persistence
- Multiple concurrent games and user accounts
- Stronger computer strategy
- API schema generation and richer integration tests