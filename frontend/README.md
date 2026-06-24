# StayFlow Cloud — Frontend

Enterprise-grade React + TypeScript SPA for the StayFlow Cloud platform. Built to the standard of a
real commercial SaaS (think Vercel / Linear / Stripe): a themeable design system, a command palette,
accessible components, professional data tables and an executive analytics dashboard.

## Stack

| Concern         | Choice                                                      |
| --------------- | ----------------------------------------------------------- |
| Framework       | React 19 + TypeScript (strict)                              |
| Build           | Vite                                                        |
| Styling         | Tailwind CSS + shadcn/ui (Radix primitives)                 |
| Server state    | TanStack Query (caching, optimistic updates, invalidation)  |
| Global UI state | Redux Toolkit (theme, sidebar, command palette)             |
| Tables          | TanStack Table (sort/filter/paginate/visibility/search/CSV) |
| Forms           | React Hook Form + Zod                                       |
| Charts          | Recharts                                                    |
| Routing         | React Router (data router, lazy routes)                     |
| HTTP            | Axios with OAuth2/JWT interceptors + silent refresh         |
| Auth            | OAuth2 / OIDC Authorization Code + PKCE (OpenIddict)        |
| Quality         | ESLint, Prettier, Husky, Conventional Commits (commitlint)  |
| Testing         | Vitest + React Testing Library, Playwright (E2E)            |

## Architecture

Feature-based, with a clear separation of concerns:

```
src/
├── app/          Providers, query client, theme applier (composition root)
├── components/
│   ├── ui/       Design system (shadcn/ui primitives)
│   ├── layout/   App shell: sidebar, topbar, command palette, theme/user menus
│   └── shared/   Reusable building blocks (DataTable, StatCard, EmptyState…)
├── features/     One folder per domain (api hooks + page + columns + dialogs)
├── hooks/        Cross-cutting hooks (useTheme, useIsMobile)
├── lib/          Pure utilities (cn, format, csv)
├── pages/        Top-level screens (login, callback, 404)
├── routes/       Router definition (lazy + code-split)
├── services/     HTTP client, auth (pkce/oidc/tokens/jwt), list helpers
├── store/        Redux Toolkit store + typed hooks
├── styles/       Tailwind layers + design tokens (light/dark)
└── types/        Shared API DTOs
```

## Run

```bash
cd frontend
cp .env.example .env      # optional; defaults work with the dev proxy
npm install
npm run dev               # http://localhost:5173
```

The backend must be running on `http://localhost:8080` (e.g. `docker compose up` from the repo root).
Vite proxies `/api`, `/connect` and `/account`, so the SPA is **same-origin** — the OAuth2 token
exchange needs no CORS. Sign in with the seeded `admin@stayflow.local` / `Admin123$`.

## Scripts

| Script              | Purpose                                       |
| ------------------- | --------------------------------------------- |
| `npm run dev`       | Dev server with HMR + API proxy               |
| `npm run build`     | Type-check then production build (code-split) |
| `npm run preview`   | Serve the production build                    |
| `npm run typecheck` | Type-check only                               |
| `npm run lint`      | ESLint                                        |
| `npm run format`    | Prettier write                                |
| `npm run test`      | Vitest (unit + RTL)                           |
| `npm run test:e2e`  | Playwright E2E                                |

## Features

- **App shell** — collapsible sidebar, responsive topbar, mobile drawer, breadcrumbs.
- **Theming** — light / dark / system, persisted, applied before paint (no flash).
- **Command palette** — ⌘K / Ctrl+K for navigation and quick actions.
- **Executive dashboard** — KPI cards (Revenue, Occupancy, ADR, RevPAR, Reservations, Guests) and
  Recharts area / bar / line / pie visualizations.
- **Professional tables** — sorting, global search, column visibility, pagination and CSV export on
  every entity (reservations, rooms, room types, guests, services, invoices, audit, documents).
- **Reservations** — create (RHF + Zod validation), cancel (optimistic) and generate invoice, with
  toast feedback.
- **Accessibility** — keyboard navigation, visible focus rings, labelled controls, semantic roles.
- **Performance** — route-level code splitting, lazy loading, Suspense, manual vendor chunks.

## Testing

- **Unit / component** — Vitest + React Testing Library (`*.test.ts(x)` next to sources).
- **E2E** — Playwright specs in `e2e/` cover login, dashboard, and the reservation lifecycle
  (create, cancel, invoice). They seed a session via the password grant and require the backend +
  dev server running: `npm run test:e2e`.

## Code quality

ESLint + Prettier enforce style. Husky runs `lint-staged` on pre-commit and `commitlint`
(Conventional Commits) on commit-msg. Hooks live in `.husky/`; enable them once per clone with:

```bash
git config core.hooksPath frontend/.husky
```

> Note: the dev-only advisories reported by `npm audit` come from the Vite/Vitest dev server and test
> runner; they are not part of the production bundle.
