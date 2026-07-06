# StayFlow Cloud — Architecture Overview

## What Is StayFlow Cloud?

StayFlow Cloud is a **multi-tenant SaaS hospitality management platform** designed for hotels and accommodation businesses. It handles reservations, front desk operations, housekeeping, billing, guest management, analytics, and document storage — all isolated per hotel tenant.

---

## High-Level System Context

```mermaid
graph TB
    Guest["🧑 Hotel Guest\n(Guest Portal)"]
    Staff["👤 Front Desk / Staff\n(Dashboard)"]
    Admin["🏨 Tenant Admin / Owner\n(Configuration)"]
    Platform["⚙️ Platform Owner\n(Tenant Management)"]

    subgraph "StayFlow Cloud"
        Web["Next.js 16 BFF\n(Frontend + API Routes)"]
        API["ASP.NET Core 9 API\n(Business Logic + OIDC Server)"]
    end

    DB[("PostgreSQL\n(Neon Serverless)")]
    Storage["S3-Compatible\nObject Storage"]
    ACA["Azure Container Apps\n(Hosting)"]

    Guest --> Web
    Staff --> Web
    Admin --> Web
    Platform --> API
    Web -->|Server-to-server REST| API
    API --> DB
    API --> Storage
    Web -.->|deployed on| ACA
    API -.->|deployed on| ACA
```

---

## Component Architecture

```mermaid
graph TB
    subgraph Frontend["Frontend — Next.js 16 App Router (BFF)"]
        Pages["App Router Pages\n(Server Components)"]
        APIRoutes["API Routes\n/api/auth/*"]
        Middleware["Edge Middleware\n(Token Refresh)"]
        PKCEFlow["PKCE Auth Flow\n(httpOnly Cookies)"]
    end

    subgraph BackendAPI["Backend — ASP.NET Core 9"]
        Controllers["REST Controllers"]
        subgraph Application["Application Layer"]
            Commands["Commands / Queries\n(MediatR CQRS)"]
            Handlers["Handlers"]
        end
        subgraph Domain["Domain Layer"]
            Aggregates["Aggregates\n(Reservation, Invoice, Guest...)"]
            DomainEvents["Domain Events"]
        end
        subgraph Infrastructure["Infrastructure"]
            EF["EF Core 9\n+ Multi-Tenant Filter"]
            Hangfire["Hangfire\nBackground Jobs"]
            MassTransit["MassTransit\nEvent Bus"]
            S3Client["S3 Document Storage"]
        end
        subgraph Auth["Auth Stack"]
            OpenIddict["OpenIddict\n(OIDC Server)"]
            Identity["ASP.NET Identity\n(Users + Roles)"]
        end
    end

    DB[("PostgreSQL\nNeon")]
    Storage["S3 Storage"]

    Pages -->|fetch| APIRoutes
    APIRoutes -->|token exchange| OpenIddict
    Middleware -->|refresh| APIRoutes
    Pages -->|server-side data| Controllers
    Controllers --> Commands
    Commands --> Handlers
    Handlers --> Aggregates
    Handlers --> EF
    Aggregates --> DomainEvents
    DomainEvents --> MassTransit
    Handlers --> Hangfire
    Handlers --> S3Client
    EF --> DB
    S3Client --> Storage
    OpenIddict --> Identity
    Identity --> EF
```

---

## Deployment Architecture

```mermaid
graph LR
    Dev["Developer\nWorkstation"]
    GH["GitHub\nRepository"]
    ACR["Azure Container\nRegistry (ACR)"]

    subgraph ACA["Azure Container Apps Environment"]
        WebApp["stayflow-prod-web\n(Next.js - Port 3000)"]
        APIApp["stayflow-prod-api\n(ASP.NET Core - Port 8080)"]
        ManagedId["User-Assigned\nManaged Identity\n(ACR Pull)"]
    end

    Neon[("Neon PostgreSQL\n(Serverless)")]
    S3["S3-Compatible\nStorage"]
    Internet["🌐 Internet"]

    Dev -->|git push| GH
    GH -->|docker build + push| ACR
    ACR -->|image pull via MI| WebApp
    ACR -->|image pull via MI| APIApp
    ManagedId -.->|authenticates| ACR
    WebApp -->|server-to-server| APIApp
    APIApp --> Neon
    APIApp --> S3
    Internet -->|HTTPS| WebApp
    Internet -->|HTTPS| APIApp
```

---

## Multi-Tenancy Model

- Every business entity carries a `TenantId` (GUID).
- `ITenantProvider` extracts the current tenant from the authenticated user's JWT claims.
- EF Core global query filters automatically scope all queries — no manual `WHERE TenantId = X` needed.
- Tenant isolation is enforced at the database **row level**.
- Platform-level operations bypass tenant filtering via admin authorization policies.

---

## Domain Modules

| Module | Responsibility |
|--------|---------------|
| **Reservations** | Booking lifecycle, room assignment, check-in/check-out |
| **Rooms & Room Types** | Room configuration, status, maintenance |
| **Guests** | Guest profiles, history, loyalty data |
| **Billing / Invoices** | Invoice generation, line items, payments |
| **Housekeeping** | Cleaning tasks, room status transitions |
| **Maintenance** | Issue tracking for rooms |
| **Orders & Services** | In-stay services (room service, amenities) |
| **Staff** | Hotel staff management, roles and permissions |
| **Analytics & Reports** | Occupancy, revenue dashboards, export |
| **Documents** | File upload/download per entity |
| **Tenants** | Tenant provisioning and feature flags |

---

## Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend | Next.js (App Router, Turbopack) | 16.x |
| Frontend Language | TypeScript | 5.x |
| Frontend Styles | Tailwind CSS + shadcn/ui | — |
| Backend Framework | ASP.NET Core | 9.0 |
| Backend Language | C# | 13 |
| Authentication | OpenIddict (OIDC) + ASP.NET Identity | 5.x |
| CQRS / Mediator | MediatR | — |
| ORM | Entity Framework Core | 9.x |
| Background Jobs | Hangfire | — |
| Event Bus | MassTransit (loopback) | — |
| Database | PostgreSQL via Neon (serverless) | 17 |
| Object Storage | S3-Compatible | — |
| Container Platform | Azure Container Apps | — |
| Image Registry | Azure Container Registry | — |
| IaC | Azure Bicep | — |
| CI/CD | GitHub Actions | — |
