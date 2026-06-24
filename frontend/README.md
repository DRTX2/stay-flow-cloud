# StayFlow Cloud — Frontend

React + TypeScript SPA for the StayFlow Cloud platform. Built with Vite, React Router and TanStack
Query, authenticating against the backend via **OAuth2 Authorization Code + PKCE** (OpenIddict).

## Stack

- React 18 + TypeScript (strict)
- Vite
- React Router v6
- TanStack Query (server state, caching, retries)
- Axios (bearer injection + silent refresh on 401)
- PKCE implemented with the Web Crypto API (no auth dependency)

## Run

```bash
cd frontend
cp .env.example .env       # optional; defaults work with the dev proxy
npm install
npm run dev                # http://localhost:5173
```

The backend must be running on `http://localhost:8080` (e.g. `docker compose up` from the repo root).
Vite proxies `/api`, `/connect` and `/account` to it, so the SPA is **same-origin** — the token
exchange needs no CORS configuration.

Sign in with the seeded account `admin@stayflow.local` / `Admin123$`. The SPA uses the seeded public
client `stayflow-spa` with redirect URI `http://localhost:5173/callback`.

## Scripts

| Script | Purpose |
|---|---|
| `npm run dev` | Dev server with HMR + API proxy |
| `npm run build` | Type-check then production build to `dist/` |
| `npm run preview` | Serve the production build locally |
| `npm run typecheck` | Type-check only |

## Structure

```
src/
  config.ts            Env-driven config (API URL, OIDC client)
  auth/                PKCE, OIDC flow, token storage, AuthContext
  api/                 Axios client (+ refresh), DTOs, TanStack Query hooks
  components/          Layout, ProtectedRoute, DataTable
  pages/               Login, Callback, Dashboard, Reservations, Rooms,
                       Guests, Services, Reports (CSV export)
```

## Auth flow

1. `Login` → `beginLogin()` builds the `/connect/authorize` URL with a PKCE challenge and redirects.
2. The API authenticates the user and redirects back to `/callback?code=…&state=…`.
3. `Callback` validates `state`, exchanges the code (+ verifier) at `/connect/token`, stores the token
   set and routes into the app.
4. The Axios client attaches the bearer token and silently refreshes once on a 401.

Routes are permission-aware: pages that need claims like `analytics:view` surface a clear message when
the signed-in user lacks them.
