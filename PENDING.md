# Pending work

StayFlow should read as a focused hotel SaaS first and a cloud engineering showcase second. The next
work is sequenced around product proof, then production hardening, then advanced differentiators.

## Product proof

The three workflows below are the product. Every new feature should improve one of them directly.

| Priority | Flow | What would make it stronger |
|---|---|---|
| P0 | Launch a property | Guided setup checklist for rooms, room types, services, staff roles and tenant features. |
| P0 | Booking to checkout | One polished demo path from hotel selection -> booking -> dashboard reservation -> check-in -> service order -> invoice. |
| P0 | Daily operations | Unified operations board combining room status, housekeeping, maintenance and F&B orders. |

## Production hardening

- Keep migrations and seed out of API startup through `StayFlow.MigrationHost` and run it as an explicit
  deployment step.
- Use separate database users for migrator and runtime application access.
- Move Azure PostgreSQL behind private networking once the student-friendly deployment is stable.
- Add staging -> production promotion with required reviewers and environment-specific configuration.
- Add SBOM generation, image signing and provenance verification before deployment.
- Add DAST/smoke tests against deployed staging.

## Later differentiators

- Per-tenant subscription plans and usage metering.
- AI assistant / semantic search only after the reservation and operations flows feel complete.
- Further microservice extraction only when a real operational seam justifies it.
- Real-time operations board with SignalR once daily operations needs push updates.

See [`docs/IMPROVEMENTS.md`](docs/IMPROVEMENTS.md) for the broader enhancement catalog.
