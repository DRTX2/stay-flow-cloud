# StayFlow Cloud

Multi-tenant **hotel operating system** for modern hospitality teams. StayFlow Cloud focuses on three
product flows: launch a property, move a guest from booking to checkout, and run daily front-desk,
housekeeping, maintenance, billing and reporting operations from one tenant-isolated workspace.

> The cloud architecture, security and DevSecOps work exists to support the hotel workflow, not to be the product.

---

## Table of contents

- [Product focus](#product-focus)
- [Architecture](#architecture)
- [Tech stack](#tech-stack)
- [Domain & modules](#domain--modules)
- [Multi-tenancy & security](#multi-tenancy--security)
- [Authentication flows](#authentication-flows)
- [Public API](#public-api)
- [Run it locally](#run-it-locally)
- [Observability](#observability)
- [Background jobs & messaging](#background-jobs--messaging)
- [Testing](#testing)
- [CI/CD](#cicd)
- [Deploy to Azure](#deploy-to-azure)
- [Deploy to AWS](#deploy-to-aws)
- [Roadmap](#roadmap)

---

## Repository layout

```
backend/     ASP.NET Core solution (StayFlowCloud.sln) — see Architecture below
frontend/    Next.js 16 + React 19 (App Router, RSC, BFF auth, Tailwind, shadcn/ui) — see frontend/README.md
deploy/       Azure Bicep, Terraform (AWS), Prometheus/Grafana provisioning
docs/         Architecture notes and the improvement analysis
compose.yaml  Full local stack (web + API + Postgres + Redis + Mongo + RabbitMQ + Prometheus + Grafana)
```

Each `backend/` and `frontend/` is independently buildable; the split is the first step toward
extracting bounded contexts into standalone services later (see `docs/IMPROVEMENTS.md`).

## Product focus

StayFlow is intentionally positioned as SaaS for hotel operators, not as a technology showcase. The
product narrative is built around three workflows:

| Flow | User outcome | Product surfaces |
|---|---|---|
| Launch a property | A hotel can configure inventory, pricing, roles and enabled modules quickly. | Tenants, rooms, room types, services, tenant features, RBAC. |
| Booking to checkout | A guest can book, staff can manage the stay, and finance can invoice cleanly. | Public booking, reservations lifecycle, guest portal, orders, invoices. |
| Daily operations | Staff can coordinate the work that keeps rooms sellable and guests moving. | Front Desk Today board, housekeeping, maintenance, room status, reports, audit. |

The technical platform matters because these flows require tenant isolation, secure auth, reliable
background jobs, observable operations and repeatable deployments. Those capabilities are supporting
evidence, not the lead story.

## Architecture

Clean Architecture with CQRS (projects under `backend/`). Dependencies point inward; the domain knows
nothing about EF, HTTP or cloud providers.

```
StayFlow.Domain          Entities, value objects, domain events. No dependencies.
StayFlow.Application      Use cases as vertical slices (CQRS + MediatR), ports (interfaces),
                         validation, cross-cutting behaviors. Depends only on Domain.
StayFlow.Persistence     EF Core DbContext, configurations, migrations, outbox, interceptors.
StayFlow.Infrastructure  Adapters: Identity/OpenIddict, Mongo audit, Redis, MassTransit,
                         Hangfire, S3/local storage, notifications, social login.
StayFlow.Api             Thin controllers, DI composition root, auth endpoints, observability.
```

Patterns in use: **CQRS + MediatR**, **vertical slices**, **domain events** (relayed via a
**transactional outbox**), **repository + unit of work** (EF `DbContext`/`SaveChanges`), and pipeline
behaviors for validation and logging.

---

## Tech stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| Primary DB | PostgreSQL + EF Core |
| Audit sink | MongoDB (optional) |
| Distributed cache / key store | Redis (optional; PostgreSQL fallback for Data Protection) |
| AuthN/AuthZ | ASP.NET Identity + OpenIddict (OAuth2 / OIDC) |
| Messaging | MassTransit + transactional outbox |
| Background jobs | Hangfire (PostgreSQL storage) |
| Documents | AWS S3 (local filesystem fallback) |
| Observability | OpenTelemetry, Serilog, Prometheus, Grafana |
| Testing | xUnit, Testcontainers, WebApplicationFactory, FluentAssertions |
| Packaging | Docker, Docker Compose |
| IaC | Azure Bicep (Container Apps with GHCR images) + Terraform reference for AWS |
| CI/CD | GitHub Actions, CodeQL, Trivy, Dependabot, Azure OIDC |

Dependencies are governed centrally via `Directory.Packages.props` (Central Package Management) with
`TreatWarningsAsErrors` and NuGet vulnerability auditing enabled.

---

## Domain & modules

- **Tenants** — property onboarding, per-tenant configuration and feature entitlements.
- **Rooms & Room Types** — inventory, pricing baselines.
- **Guests** — guest profiles per tenant.
- **Reservations** — booking lifecycle with a `DateRange` (check-in/check-out) value object; raises
  domain events on confirm/cancel.
- **Housekeeping & Maintenance** — room readiness, work orders and operational follow-through.
- **Orders** — F&B / service orders attached to active stays.
- **Invoices** — billing tied to reservations and services.
- **Services** — sellable extras attached to stays.
- **Analytics** — dashboard summary and revenue reporting.
- **Audit** — tenant audit records can be persisted to MongoDB when that optional sink is configured.
- **Tenant Features** — per-tenant feature flags.
- **Documents** — tenant-scoped file storage (S3/local) with cross-tenant access guards.

---

## Multi-tenancy & security

- **Tenant isolation** — every tenant-owned entity carries a `TenantId`; EF Core **global query
  filters** scope all reads automatically, and the outbox interceptor stamps tenant + user onto each
  message so async work stays in-tenant.
- **RBAC** — six roles plus granular permission claims (`reservations:read`, `analytics:view`,
  `documents:write`, …). Controllers authorize on permissions, not roles, so access is fine-grained.
- **Defense in depth** — document endpoints verify tenant ownership before serving a key; storage
  adapters guard against path traversal; secrets come from configuration / managed secrets, never
  source.

---

## Authentication flows

OpenIddict exposes a standards-compliant OAuth2/OIDC server:

| Flow | Endpoint | Use |
|---|---|---|
| Client Credentials | `POST /connect/token` (`grant_type=client_credentials`) | Machine-to-machine / public API |
| Refresh Token | `POST /connect/token` (`grant_type=refresh_token`) | Token rotation |
| Authorization Code + PKCE | `GET /connect/authorize` → `POST /connect/token` | Next.js BFF / mobile (see seeded `spa` client) |
| Social login | `/account/external` (Google / Microsoft / GitHub) | Federated sign-in (enabled when configured) |

Supporting endpoints: `/connect/userinfo` (scope-gated claims), `/connect/logout`, `/account/login`.
The seeded SPA client redirects to `http://localhost:3000/api/auth/callback` (the Next.js BFF, which
completes the PKCE exchange server-side and stores tokens in httpOnly cookies) and requires PKCE.
Password / ROPC is intentionally not part of the interactive login path.

---

## Public API

Representative resource endpoints (all tenant-scoped, permission-gated):

```
GET    /api/v1/reservations         POST /api/v1/reservations
POST   /api/v1/reservations/{id}/{confirm|check-in|check-out|cancel}
GET    /api/v1/rooms                POST /api/v1/rooms     PUT /api/v1/rooms/{id}/price
GET    /api/v1/roomtypes            POST /api/v1/roomtypes PUT /api/v1/roomtypes/{id}
GET    /api/v1/guests               POST /api/v1/guests    PUT /api/v1/guests/{id}
GET    /api/v1/services             POST /api/v1/services  PUT /api/v1/services/{id}
GET    /api/v1/orders               POST /api/v1/orders    POST /api/v1/orders/{id}/deliver
GET    /api/v1/housekeeping         POST /api/v1/housekeeping
GET    /api/v1/maintenance          POST /api/v1/maintenance
GET    /api/v1/invoices             POST /api/v1/invoices  POST /api/v1/invoices/{id}/pay
GET    /api/v1/analytics/dashboard  GET  /api/v1/analytics/revenue
GET    /api/v1/analytics/front-desk/today
GET    /api/v1/audit
GET    /api/v1/tenantfeatures
POST   /api/v1/documents            GET  /api/v1/documents/{key}
GET    /api/v1/tenants              POST /api/v1/tenants
```

A contract test suite (`StayFlow.ContractTests`) pins the shape of the public surface so breaking
changes fail CI.

---

## Run it locally

Everything (web + API + database lifecycle job + PostgreSQL + Redis + MongoDB + RabbitMQ + Prometheus + Grafana) comes up with one command:

```bash
make up
```

Useful local commands:

| Command | Purpose |
|---|---|
| `make up` | Build and start the full stack. |
| `make down` | Stop the stack. |
| `make fresh` | Reset, migrate and seed the local database. |
| `make test` | Run backend and frontend tests. |
| `make docker-build` | Build API, MigrationHost, NotificationService and web images. |

| Service | URL |
|---|---|
| API | http://localhost:8080 |
| Scalar API reference | http://localhost:8080/docs |
| OpenAPI JSON | http://localhost:8080/openapi/v1.json |
| Health (liveness) | http://localhost:8080/health/live |
| Health (readiness) | http://localhost:8080/health/ready |
| Metrics | http://localhost:8080/metrics |
| Hangfire dashboard | http://localhost:8080/hangfire |
| Prometheus | http://localhost:9090 |
| Grafana | http://localhost:3001 (admin / admin) |

Backing services also publish their ports: PostgreSQL `5432`, Redis `6379`, MongoDB `27017`.

To run the API directly against your own infrastructure, set the connection strings and run:

```bash
dotnet run --project backend/StayFlow.Api
```

Database lifecycle is explicit. Use `StayFlow.MigrationHost` for migrations/seeding instead of doing
schema changes on API startup:

```bash
dotnet run --project backend/StayFlow.MigrationHost -- migrate
dotnet run --project backend/StayFlow.MigrationHost -- seed
```

### Frontend

```bash
cd frontend
npm install
npm run dev        # http://localhost:3000
```

The Next.js app proxies API/auth calls to the backend on `:8080` and completes OAuth server-side
(BFF; tokens in httpOnly cookies). The public marketing/booking site is at `/`, the dashboard at
`/dashboard`. For local seeded credentials, use your `.env` values or the development-only
defaults documented in `backend/StayFlow.Api/appsettings.Development.json`. See
[`frontend/README.md`](frontend/README.md).

---

## Observability

- **Metrics** — OpenTelemetry instruments ASP.NET Core, HTTP clients, EF Core and the runtime, and
  exposes them at `/metrics` for Prometheus to scrape (`deploy/prometheus.yml`).
- **Dashboards** — Grafana is pre-provisioned with Prometheus, Loki and Tempo datasources and a StayFlow API
  dashboard (request rate, p95 latency, 5xx rate, process memory) under `deploy/grafana`.
- **Tracing** — OTLP export is wired and activates automatically when an OTLP endpoint is configured.
- **Logging** — Serilog structured logs.
- **Health** — `/health/live` and `/health/ready` check the process and configured dependencies
  (PostgreSQL, Redis and MongoDB) for Compose, Container Apps and other orchestrators.

---

## Background jobs & messaging

- **Outbox** — domain events are written to an `OutboxMessages` table in the same transaction as the
  business change; a background processor relays them to MassTransit, so no event is lost on crash.
- **MassTransit** — in-memory transport by default; swap the transport (RabbitMQ/SQS) without touching
  producers or consumers.
- **Hangfire** — recurring jobs on PostgreSQL storage: occupancy calculation, night audit, reminder
  emails, invoice generation and outbox cleanup. Dashboard at `/hangfire`.

---

## Testing

| Suite | Project | Notes |
|---|---|---|
| Unit | `StayFlow.UnitTests` | Domain + application logic. Runs anywhere. |
| Integration | `StayFlow.IntegrationTests` | Spins up real PostgreSQL via Testcontainers + `WebApplicationFactory`. |
| Contract | `StayFlow.ContractTests` | Locks the public API shape. |

```bash
dotnet test
```

> Integration and contract suites require the ASP.NET Core shared runtime and Docker; they run in CI.
> Unit tests run on any machine with the .NET SDK.

---

## CI/CD

GitHub Actions provides the DevSecOps baseline:

1. **ci.yml** — restore/build/test backend, lint/typecheck/test/build frontend, run Playwright E2E, CodeQL, secret/IaC scans, Docker builds and Trivy image gates.
2. **dependency-review.yml** — blocks high-risk dependency changes in PRs.
3. **deploy-azure.yml** — provisions Azure with Bicep, builds immutable images, pushes to GHCR and deploys Container Apps using GitHub OIDC.
5. **dependabot.yml** — keeps GitHub Actions, NuGet, npm and Docker bases current.

See [`docs/DEVSECOPS.md`](docs/DEVSECOPS.md) for the control map.

---

## Deploy to Azure

The primary student-friendly deployment path is Azure Container Apps + Azure Database for PostgreSQL:

```bash
az group create --name rg-stayflow-dev --location eastus
az deployment group create \
  --resource-group rg-stayflow-dev \
  --template-file deploy/azure/main.bicep
```

The automated path is `.github/workflows/deploy-azure.yml`. It publishes images to GHCR instead of ACR to avoid Azure Container Registry charges. Configure the required GitHub secrets from [`deploy/azure/README.md`](deploy/azure/README.md), then push to `main` or run the workflow manually.

---

## Deploy to AWS

Production-shaped Terraform lives in [`deploy/terraform`](deploy/terraform/README.md): VPC with public
and private subnets, ECS Fargate behind an ALB, RDS PostgreSQL, ElastiCache Redis, an S3 documents
bucket fronted by CloudFront, connection strings in Secrets Manager and least-privilege IAM roles.

```bash
cd deploy/terraform
terraform init
terraform apply -var "container_image=ghcr.io/<owner>/stayflowcloud-api:latest"
terraform output api_url
```

See the [Terraform README](deploy/terraform/README.md) for backend, TLS and sizing notes.

---

## Roadmap

Backend, a Next.js 16 frontend (public booking, guest portal and staff dashboard), operations modules,
infrastructure and CI/CD are in place. Planned next steps are tracked in [`PENDING.md`](PENDING.md):
deepen the three core product flows first, then harden production networking, promotion gates and
supply-chain controls.
