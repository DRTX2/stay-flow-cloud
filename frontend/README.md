# StayFlow Cloud — Frontend (Next.js)

Product frontend for StayFlow Cloud, a modern hotel operating system built around three core flows:
property onboarding, reservation-to-checkout, and daily hotel operations. It combines the public
booking/guest portal with the authenticated staff dashboard, talking to the .NET API through a
**Backend-for-Frontend (BFF)** auth layer.

## Stack

- **Next.js 16** App Router (Server Components, Server Actions, streaming), **Turbopack**
- **React 19** + **React Compiler** (auto-memoization)
- **TypeScript** strict (`noUncheckedIndexedAccess`)
- **Tailwind CSS 3.4** + **shadcn/ui** (hand-authored Radix components)
- **TanStack Table** (data grid), **Recharts** (analytics), **React Hook Form + Zod** (forms)
- **next-themes** (light/dark/system), **sonner** (toasts), **cmdk** (command palette)
- **jose** (JWT decode), **Axios-free** server fetch client
- **ESLint** (eslint-config-next flat) · **Prettier** · **Husky** + **commitlint** · **lint-staged**
- **Vitest** + Testing Library (unit) · **Playwright** (e2e)

## Architecture

```
src/
  app/
    (public)/        Marketing landing, pricing, hotels (ISR), hotel detail, booking
    (auth)/login/    Branded sign-in entry
    dashboard/       Authenticated app (RSC pages + server actions + layout/loading/error)
    api/auth/        BFF route handlers: login (PKCE start), callback, logout
    sitemap.ts robots.ts manifest.ts
  components/
    ui/              shadcn/ui primitives
    layout/          Sidebar, Topbar, CommandPalette, AppShell, theme/user menus
    shared/          StatCard, EmptyState, PageHeader, DataTable, …
    public/          Public header/footer
  features/          Per-domain UI (dashboard, reservations, rooms, booking, …)
  server/            BFF: config, API client, auth (pkce/oidc/session/cookies/current-user)
  content/           Curated public hotel catalog (CMS-style source for SSG/ISR)
  proxy.ts           Next middleware: gatekeeps /dashboard, refreshes tokens
```

### Rendering strategy

| Area                       | Strategy                                                 |
| -------------------------- | -------------------------------------------------------- |
| Marketing landing, pricing | **SSG** (static)                                         |
| Hotels list, hotel detail  | **ISR** (`revalidate`, `generateStaticParams`) + JSON-LD |
| Dashboard + entity pages   | **SSR / RSC**, streamed with Suspense                    |
| Mutations                  | **Server Actions** + `revalidatePath`                    |

### Auth (BFF)

OAuth2 **Authorization Code + PKCE** runs entirely **server-side**. The verifier/state live in
short-lived httpOnly cookies; the code is exchanged for tokens server-to-server; access/refresh
tokens are stored in **httpOnly cookies never exposed to JavaScript**. `proxy.ts` refreshes expired
tokens before rendering `/dashboard`; server components/actions read the token via the cookie and
forward it as a bearer to the API.

## Develop

```bash
npm install
cp .env.example .env.local   # adjust if your API isn't on :8080
npm run dev                  # http://localhost:3000
```

Bring the backend up first (`docker compose up` from the repo root). The dev server proxies
`/connect/*` and `/api/backend/*` to the API. Sign in with the seeded local admin configured via
`.env`/`appsettings.Development.json`; never reuse local demo credentials in production.

## Scripts

| Script                    | Description                        |
| ------------------------- | ---------------------------------- |
| `npm run dev`             | Dev server (Turbopack)             |
| `npm run build` / `start` | Production build / serve           |
| `npm run typecheck`       | `tsc --noEmit`                     |
| `npm run lint` / `format` | ESLint / Prettier                  |
| `npm test`                | Vitest unit tests                  |
| `npm run test:e2e`        | Playwright (needs a running stack) |

## Environment

| Var                             | Scope  | Purpose                                           |
| ------------------------------- | ------ | ------------------------------------------------- |
| `NEXT_PUBLIC_SITE_URL`          | public | Canonical URL (metadata, OAuth redirect, sitemap) |
| `NEXT_PUBLIC_OIDC_AUTHORITY`    | public | Browser-facing OAuth endpoints                    |
| `API_INTERNAL_URL`              | server | Server-to-server API base (data + token exchange) |
| `OIDC_CLIENT_ID` / `OIDC_SCOPE` | server | OAuth client + scopes                             |

## Docker

`docker compose up --build` runs the app as the `web` service on
[http://localhost:3000](http://localhost:3000) (standalone Next output).
