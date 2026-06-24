# Pending work

Snapshot of what remains after the backend/infra/tooling are complete.

## Not yet built

- **Microservice extraction** — pull a bounded context (Notifications or Analytics) out of the
  monolith as a standalone service over the existing outbox/integration events. This is the agreed
  next step (`docs/IMPROVEMENTS.md §1`).
- **Run Playwright E2E against a live stack** — specs exist (`frontend/e2e`: login, dashboard,
  reservation lifecycle); wire them into CI with the backend + dev server running.
- **Frontend depth** — the enterprise SPA covers auth, the executive dashboard and full CRUD-style
  tables for every entity (create/cancel/invoice on reservations). Still to add: edit forms for the
  other entities, the guest-facing customer portal, and server-side pagination for large datasets.

## Nice-to-have hardening

- Real OTLP/tracing backend (Tempo/Jaeger) wired in Compose for end-to-end traces.
- Swap MassTransit in-memory transport for RabbitMQ or Amazon SQS.
- ACM certificate + HTTPS listener + Route 53 record in the Terraform stack.
- Remote Terraform backend (S3 + DynamoDB lock) before any team use.
- MongoDB target for production (DocumentDB or Atlas) — currently optional with a no-op fallback.

## Future direction

See [`docs/IMPROVEMENTS.md`](docs/IMPROVEMENTS.md) for the full enhancement analysis
(event-driven microservices, gRPC, semantic search / RAG, Kubernetes, DevSecOps, cloud networking).
