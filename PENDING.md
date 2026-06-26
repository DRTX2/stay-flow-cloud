# Pending work

Snapshot of what remains after the backend/infra/tooling are complete.

## Not yet built

- **Further microservice extraction** — the Notification service is extracted (RabbitMQ consumer over
  the outbox/integration events); next pull Analytics or Billing out of the monolith
  (`docs/IMPROVEMENTS.md §1`).

The dashboard is now full CRUD: create **and edit** dialogs for rooms, room types, guests and
services (backend `PUT /api/v1/{guests,services,roomtypes}/{id}`), the full reservation lifecycle
(confirm / check-in / check-out / cancel / generate invoice), room base-price updates, invoice
"mark as paid", and tenant-feature toggles. Reservations, guests, rooms and invoices use
**server-side pagination** (deep-linkable `?page/?pageSize/?search`). The Playwright E2E suite runs
in CI against a live Compose-backed API (`e2e` job). Remaining frontend depth is optional polish
(bulk actions, richer filtering, a customer self-service portal).

## Nice-to-have hardening

- Real OTLP/tracing backend (Tempo/Jaeger) wired in Compose for end-to-end traces.
- Swap MassTransit in-memory transport for RabbitMQ or Amazon SQS.
- ACM certificate + HTTPS listener + Route 53 record in the Terraform stack.
- Remote Terraform backend (S3 + DynamoDB lock) before any team use.
- MongoDB target for production (DocumentDB or Atlas) — currently optional with a no-op fallback.

## Future direction

See [`docs/IMPROVEMENTS.md`](docs/IMPROVEMENTS.md) for the full enhancement analysis
(event-driven microservices, gRPC, semantic search / RAG, Kubernetes, DevSecOps, cloud networking).
