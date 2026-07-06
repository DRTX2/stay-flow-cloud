# StayFlow Cloud — Sequence Diagrams

## 1. Authentication Flow (OIDC Authorization Code + PKCE)

```mermaid
sequenceDiagram
    actor Browser
    participant BFF as Next.js BFF
    participant API as ASP.NET Core API<br/>(OpenIddict)
    participant DB as PostgreSQL

    Browser->>BFF: GET /api/auth/login?redirect=/dashboard
    BFF->>BFF: Generate PKCE verifier + code_challenge + state
    BFF->>Browser: Set httpOnly cookies (verifier, state, returnTo)
    BFF-->>Browser: 302 Redirect → /connect/authorize?client_id=...&code_challenge=...
    Browser->>API: GET /connect/authorize?...
    API->>API: Validate client_id, redirect_uri, scope
    Note over API: User not authenticated (no Identity cookie)
    API-->>Browser: 302 Redirect → https://web.app/signin?ReturnUrl=/connect/authorize?...
    Browser->>BFF: GET /signin (credential form)
    BFF-->>Browser: 200 HTML sign-in form

    Browser->>API: POST /account/login {email, password, returnUrl}
    API->>DB: Lookup user by email
    DB-->>API: ApplicationUser record
    API->>API: PasswordSignInAsync (bcrypt verify)
    API->>Browser: Set StayFlow.Identity cookie (session)
    API-->>Browser: 302 Redirect → /connect/authorize?... (ReturnUrl)

    Browser->>API: GET /connect/authorize?... (now authenticated)
    API->>DB: Create authorization code record
    DB-->>API: code stored
    API-->>Browser: 302 Redirect → /api/auth/callback?code=AUTH_CODE&state=...

    Browser->>BFF: GET /api/auth/callback?code=AUTH_CODE&state=...
    BFF->>BFF: Verify state matches cookie
    BFF->>API: POST /connect/token {code, verifier, client_id, redirect_uri}
    API->>DB: Validate code + verifier (PKCE)
    DB-->>API: code valid
    API->>DB: Create access + refresh token records
    DB-->>API: tokens
    API-->>BFF: {access_token, refresh_token, expires_in}
    BFF->>Browser: Set httpOnly cookies (access, refresh, expiry)
    BFF-->>Browser: 302 Redirect → /dashboard
```

---

## 2. Authenticated API Request (Server-to-Server)

```mermaid
sequenceDiagram
    actor Browser
    participant BFF as Next.js BFF<br/>(Server Component)
    participant API as ASP.NET Core API
    participant DB as PostgreSQL

    Browser->>BFF: GET /dashboard/reservations
    BFF->>BFF: Read access_token from httpOnly cookie
    BFF->>BFF: Check expiry — if < 60s remaining, refresh first
    BFF->>API: GET /api/v1/reservations<br/>Authorization: Bearer {access_token}
    API->>API: OpenIddict validates JWT signature
    API->>API: Extract TenantId from claims → ITenantProvider
    API->>DB: SELECT * FROM Reservations WHERE TenantId = '{id}'
    Note over DB: EF Core global query filter applied automatically
    DB-->>API: Reservation[]
    API-->>BFF: 200 JSON
    BFF-->>Browser: Rendered HTML (RSC)
```

---

## 3. Token Refresh Flow (Edge Middleware)

```mermaid
sequenceDiagram
    actor Browser
    participant Middleware as Next.js Edge Middleware
    participant BFF as Next.js API Route
    participant API as ASP.NET Core API<br/>(OpenIddict)

    Browser->>Middleware: Any authenticated request
    Middleware->>Middleware: Read expiry cookie
    alt Token expires in < 60 seconds
        Middleware->>BFF: POST /api/auth/refresh (internal)
        BFF->>API: POST /connect/token<br/>{grant_type: refresh_token, refresh_token: ...}
        API->>API: Validate refresh token
        API-->>BFF: New {access_token, refresh_token, expires_in}
        BFF->>Browser: Update httpOnly cookies
    end
    Middleware->>Browser: Continue to requested route
```

---

## 4. Reservation Creation Flow

```mermaid
sequenceDiagram
    actor Staff as Front Desk Staff
    participant BFF as Next.js BFF
    participant API as ASP.NET Core API
    participant MediatR as MediatR
    participant DB as PostgreSQL
    participant EventBus as MassTransit

    Staff->>BFF: POST /dashboard/reservations/new (form submit)
    BFF->>API: POST /api/v1/reservations<br/>Bearer {access_token}
    API->>API: Authorize: RequirePermission("reservations.create")
    API->>MediatR: Send(CreateReservationCommand)
    MediatR->>DB: Check room availability for dates
    DB-->>MediatR: Room available
    MediatR->>DB: BEGIN TRANSACTION
    MediatR->>DB: INSERT Reservation (TenantId, RoomId, GuestId, dates, status=Confirmed)
    MediatR->>DB: UPDATE Room (status=Reserved)
    MediatR->>DB: COMMIT
    DB-->>MediatR: ReservationId
    MediatR->>EventBus: Publish ReservationCreatedEvent
    EventBus->>EventBus: Handle: Send confirmation notification (async)
    MediatR-->>API: CreateReservationResult {id, confirmationCode}
    API-->>BFF: 201 Created {id, confirmationCode}
    BFF-->>Staff: Redirect to reservation detail page
```

---

## 5. Invoice Generation Flow

```mermaid
sequenceDiagram
    actor Staff as Front Desk Staff
    participant API as ASP.NET Core API
    participant MediatR as MediatR
    participant DB as PostgreSQL
    participant Hangfire as Hangfire<br/>(Background)

    Staff->>API: POST /api/v1/invoices/generate<br/>{reservationId}
    API->>MediatR: Send(GenerateInvoiceCommand)
    MediatR->>DB: Load Reservation + Room + Services
    DB-->>MediatR: Reservation aggregate
    MediatR->>MediatR: Calculate line items (room nights × rate, extras)
    MediatR->>MediatR: Apply taxes
    MediatR->>DB: INSERT Invoice + InvoiceLines
    DB-->>MediatR: InvoiceId
    MediatR->>Hangfire: Enqueue SendInvoiceEmailJob(invoiceId)
    MediatR-->>API: Invoice {id, total, status=Draft}
    API-->>Staff: 201 Created Invoice

    Note over Hangfire: Background processing
    Hangfire->>DB: Load Invoice + Guest contact
    Hangfire->>Hangfire: Generate PDF
    Hangfire->>Hangfire: Send email with PDF attachment
```

---

## 6. Check-Out Flow

```mermaid
sequenceDiagram
    actor Staff as Front Desk Staff
    participant API as ASP.NET Core API
    participant MediatR as MediatR
    participant DB as PostgreSQL
    participant EventBus as MassTransit

    Staff->>API: POST /api/v1/reservations/{id}/checkout
    API->>MediatR: Send(CheckOutCommand {reservationId})
    MediatR->>DB: Load Reservation
    DB-->>MediatR: Reservation (status=CheckedIn)
    MediatR->>MediatR: Validate: can check out?
    MediatR->>DB: UPDATE Reservation (status=CheckedOut, checkOutAt=now)
    MediatR->>DB: UPDATE Room (status=Dirty)
    MediatR->>EventBus: Publish GuestCheckedOutEvent
    EventBus->>DB: Create HousekeepingTask (clean room)
    MediatR-->>API: CheckOutResult
    API-->>Staff: 200 OK
```
