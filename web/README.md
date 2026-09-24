# Smart Solar Microgrid Web

Shared React + TypeScript + Vite foundation for the Smart Solar Microgrid Trading System.

## Commands

```sh
npm install
npm run dev
npm run build
npm run lint
```

## Organization

- `src/app`: application entry composition and router
- `src/components`: reusable UI, feedback, and navigation components
- `src/layouts`: responsive application shell and page-level layouts
- `src/pages`: route-level foundation pages
- `src/features`: future domain modules
- `src/styles`: centralized tokens, global rules, utilities, and component/layout styles
- `src/services`, `src/hooks`, `src/types`, `src/utils`: shared integration and application support as later stages require them

Design tokens live in `src/styles/tokens.css`; shared primitives live in `src/components`. Feature developers should reuse these tokens and components instead of introducing unrelated styling systems or scattered literal colors.

Copy `.env.example` to a Git-ignored local `.env` for development. Variables exposed to the browser must use the `VITE_` prefix.

## API and authentication

Set `VITE_API_BASE_URL` to the backend origin; the shared Axios client in `src/services/api` trims trailing slashes, attaches the bearer token, and normalizes API failures. Feature modules must use this client instead of creating additional Axios instances.

The JWT is stored only in `sessionStorage`. On startup, the application validates a stored token through `GET /api/auth/me`; it never treats decoded or cached profile data as authoritative. Web access is restricted to `BACKOFFICE` and `GRID_OPERATOR`. A 401 clears the session, while a 403 preserves it and is handled as an authorization failure.
