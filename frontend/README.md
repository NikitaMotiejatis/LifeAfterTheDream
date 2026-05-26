# Port Risk Monitor — Frontend

React 18 + TypeScript + Vite single-page application.

## Tech Stack

- **React 18** with lazy-loaded routes
- **Vite** (dev server + build)
- **Tailwind CSS** for styling
- **TanStack React Query** for server state (auto-refetch every 30 s)
- **Axios** for HTTP requests
- **Recharts** for charts

## Getting Started

```bash
npm install
npm run dev          # http://localhost:5173
npm run build        # production bundle in dist/
```

## Project Structure

```
src/
├── api/              HTTP clients (axios-based)
├── components/       Reusable UI components
├── contexts/         React context providers (Auth, Toast)
├── hooks/            Custom hooks (useDashboard, useSettings, etc.)
├── mocks/            Static mock data for offline development
├── pages/            Route-level page components
├── types/            TypeScript type definitions
└── utils/            Pure utility functions
```

## NFR: Concurrency

No use-case data is stored in browser session. Each tab/window operates independently using
the same `sessionStorage` auth token — multiple tabs work without conflicts.

## NFR: Optimistic Locking (client side)

`axiosInstance.ts` intercepts HTTP 409 responses (concurrency conflict from backend RowVersion
mismatch) and warns the user, allowing them to refresh and retry.

## NFR: Reactive / Non-blocking

React Query keeps the UI responsive — data fetches run asynchronously with loading/error states.
The dashboard auto-refreshes every 30 seconds without blocking user interaction.
