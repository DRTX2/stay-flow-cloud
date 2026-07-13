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

- Use separate database users for migrator and runtime application access.
- Move Azure PostgreSQL behind private networking once the student-friendly deployment is stable.
- Add staging -> production promotion with required reviewers and environment-specific configuration.
- Add DAST/smoke tests against deployed staging.
- Add PostgreSQL RLS as defense in depth for tenant-owned tables.
- Add durable object storage before enabling production document uploads.

Completed foundations include explicit migration jobs, commit-gated deployment, SBOM/provenance,
persistent OpenIddict/Data Protection keys, concurrency-safe active-room reservations, Scalar/OpenAPI,
and a local OpenTelemetry/Prometheus/Loki/Tempo/Grafana stack.

## Later differentiators

- Per-tenant subscription plans and usage metering.
- Tenant knowledge base, extractive search and local Ollama/Whisper adapters after notifications and inventory.
- Further microservice extraction only when a real operational seam justifies it.
- Real-time operations board with SignalR once daily operations needs push updates.

See [`docs/IMPROVEMENTS.md`](docs/IMPROVEMENTS.md) for the broader enhancement catalog.
