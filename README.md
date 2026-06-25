# StayFlow Cloud

Multi-tenant **Hospitality Management SaaS** (hotels, hostels, resorts) built as a production-shaped
reference application. A single ASP.NET Core API serves many tenants, each with isolated data, RBAC,
feature flags and OAuth2 access — packaged as a **modular monolith** designed to peel off into
microservices.

> Portfolio project demonstrating senior-level backend & cloud engineering on .NET 10 and AWS.

---

## Table of contents

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
- [Deploy to AWS](#deploy-to-aws)
- [Roadmap](#roadmap)

---

## Repository layout

```
backend/     ASP.NET Core solution (StayFlowCloud.sln) — see Architecture below
frontend/    Next.js 16 + React 19 (App Router, RSC, BFF auth, Tailwind, shadcn/ui) — see frontend/README.md
deploy/       Terraform (AWS), Prometheus/Grafana provisioning
docs/         Architecture notes and the improvement analysis
compose.yaml  Full local stack (web + API + Postgres + Redis + Mongo + RabbitMQ + Prometheus + Grafana)
```

Each `backend/` and `frontend/` is independently buildable; the split is the first step toward
extracting bounded contexts into standalone services later (see `docs/IMPROVEMENTS.md`).

## Architecture

Clean Architecture with CQRS (projects under `backend/`). Dependencies point inward; the domain knows
nothing about EF, HTTP or AWS.

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
| Audit / event store | MongoDB |
| Cache / rate-limit / sessions | Redis (StackExchange) |
| AuthN/AuthZ | ASP.NET Identity + OpenIddict (OAuth2 / OIDC) |
| Messaging | MassTransit + transactional outbox |
| Background jobs | Hangfire (PostgreSQL storage) |
| Documents | AWS S3 (local filesystem fallback) |
| Observability | OpenTelemetry, Serilog, Prometheus, Grafana |
| Testing | xUnit, Testcontainers, WebApplicationFactory, FluentAssertions |
| Packaging | Docker, Docker Compose |
| IaC | Terraform (AWS ECS Fargate, RDS, ElastiCache, S3/CloudFront) |
| CI/CD | GitHub Actions + SonarCloud + GHCR |

Dependencies are governed centrally via `Directory.Packages.props` (Central Package Management) with
`TreatWarningsAsErrors` and NuGet vulnerability auditing enabled.

---

## Domain & modules

- **Tenants** — onboarding, per-tenant configuration.
- **Rooms & Room Types** — inventory, pricing baselines.
- **Guests** — guest profiles per tenant.
- **Reservations** — booking lifecycle with a `DateRange` (check-in/check-out) value object; raises
  domain events on confirm/cancel.
- **Invoices** — billing tied to reservations and services.
- **Services** — sellable extras attached to stays.
- **Analytics** — dashboard summary and revenue reporting.
- **Audit** — every domain event persisted to MongoDB for an immutable trail.
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
  adapters guard against path traversal; secrets come from configuration / AWS Secrets Manager, never
  source.

---

## Authentication flows

OpenIddict exposes a standards-compliant OAuth2/OIDC server:

| Flow | Endpoint | Use |
|---|---|---|
| Resource Owner Password | `POST /connect/token` (`grant_type=password`) | First-party login |
| Client Credentials | `POST /connect/token` (`grant_type=client_credentials`) | Machine-to-machine / public API |
| Refresh Token | `POST /connect/token` (`grant_type=refresh_token`) | Token rotation |
| Authorization Code + PKCE | `GET /connect/authorize` → `POST /connect/token` | Next.js BFF / mobile (see seeded `spa` client) |
| Social login | `/account/external` (Google / Microsoft / GitHub) | Federated sign-in (enabled when configured) |

Supporting endpoints: `/connect/userinfo` (scope-gated claims), `/connect/logout`, `/account/login`.
The seeded SPA client redirects to `http://localhost:3000/api/auth/callback` (the Next.js BFF, which
completes the PKCE exchange server-side and stores tokens in httpOnly cookies) and requires PKCE.

---

## Public API

Representative resource endpoints (all tenant-scoped, permission-gated):

```
GET    /api/v1/reservations         POST /api/v1/reservations
GET    /api/v1/rooms                GET  /api/v1/roomtypes
GET    /api/v1/guests               POST /api/v1/guests
GET    /api/v1/invoices
GET    /api/v1/services
GET    /api/v1/analytics/dashboard  GET  /api/v1/analytics/revenue
GET    /api/v1/audit
GET    /api/v1/tenantfeatures
POST   /api/v1/documents            GET  /api/v1/documents/{key}
GET    /api/v1/tenants              POST /api/v1/tenants
```

A contract test suite (`StayFlow.ContractTests`) pins the shape of the public surface so breaking
changes fail CI.

---

## Run it locally

Everything (API + PostgreSQL + Redis + MongoDB + Prometheus + Grafana) comes up with one command:

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| API | http://localhost:8080 |
| Swagger / OpenAPI | http://localhost:8080/swagger |
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

EF migrations apply automatically on startup; data is seeded (roles, permissions, demo tenant, OAuth
clients) on first run.

### Frontend

```bash
cd frontend
npm install
npm run dev        # http://localhost:3000
```

The Next.js app proxies API/auth calls to the backend on `:8080` and completes OAuth server-side
(BFF; tokens in httpOnly cookies). The public marketing/booking site is at `/`, the dashboard at
`/dashboard`. Sign in with the seeded `admin@stayflow.local` / `Admin123$`. See
[`frontend/README.md`](frontend/README.md).

---

## Observability

- **Metrics** — OpenTelemetry instruments ASP.NET Core, HTTP clients, EF Core and the runtime, and
  exposes them at `/metrics` for Prometheus to scrape (`deploy/prometheus.yml`).
- **Dashboards** — Grafana is pre-provisioned with the Prometheus datasource and a StayFlow API
  dashboard (request rate, p95 latency, 5xx rate, process memory) under `deploy/grafana`.
- **Tracing** — OTLP export is wired and activates automatically when an OTLP endpoint is configured.
- **Logging** — Serilog structured logs.
- **Health** — `/health/live` and `/health/ready` (the latter checks PostgreSQL, Redis, MongoDB and
  the message bus) feed both Compose and the ECS/ALB health check.

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

`.github/workflows/ci.yml`:

1. **build-test** — restore, build (warnings as errors), run tests.
2. **sonar** — SonarCloud static analysis (runs when `SONAR_TOKEN` is present).
3. **docker** — build and push the API image to GHCR on `main`.

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

Backend, a Next.js 16 frontend (public site + dashboard, BFF auth), a first extracted microservice
(notifications over RabbitMQ), infrastructure and tooling are in place. Planned next steps are tracked
in [`PENDING.md`](PENDING.md) and the [improvement analysis](docs/IMPROVEMENTS.md) — highlights:
further microservice extraction, running the Playwright E2E suite in CI, semantic search
(embeddings/RAG), and cloud networking hardening.
