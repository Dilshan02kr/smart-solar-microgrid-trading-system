# Smart Solar Microgrid Web

React + TypeScript + Vite client for the Smart Solar Microgrid Trading System.

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
- `src/pages`: shared route-level pages
- `src/features`: authentication, user, Prosumer, station, slot, reservation, and operator modules
- `src/styles`: centralized tokens, global rules, utilities, and component/layout styles
- `src/services` and `src/utils`: shared API and presentation support

Design tokens live in `src/styles/tokens.css`; shared primitives live in `src/components`. Feature developers should reuse these tokens and components instead of introducing unrelated styling systems or scattered literal colors.

Copy `.env.example` to a Git-ignored local `.env` for development. Variables exposed to the browser must use the `VITE_` prefix.

## API and authentication

Set `VITE_API_BASE_URL` to the backend origin; the shared Axios client in `src/services/api` trims trailing slashes, attaches the bearer token, and normalizes API failures. Feature modules must use this client instead of creating additional Axios instances.

The JWT is stored only in `sessionStorage`. On startup, the application validates a stored token through `GET /api/auth/me`; it never treats decoded or cached profile data as authoritative. Web access is restricted to `BACKOFFICE` and `GRID_OPERATOR`. A 401 clears the session, while a 403 preserves it and is handled as an authorization failure.

Backoffice and Grid Operator routes are protected by shared authentication and role guards. Backoffice manages users, Prosumers, microgrid nodes, slots, and reservations. Grid Operators receive a station-scoped dashboard, existing-slot availability management, reservation monitoring, and transaction verification/completion workflow.

Backoffice station create/edit forms use `VITE_GOOGLE_MAPS_API_KEY` for visual coordinate selection. Use a browser key restricted by HTTP referrer with Maps JavaScript API enabled. If the variable is empty or Maps cannot load, manual latitude/longitude entry remains available. This browser credential is separate from Android's package/SHA-restricted `MAPS_API_KEY`.

For local browser integration, configure the backend's existing `Cors:AllowedOrigins` setting to include the Vite origin, commonly `http://localhost:5173`. Keep the origin explicit; do not enable unrestricted CORS.
