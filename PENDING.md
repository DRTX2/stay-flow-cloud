# Pending work

Snapshot of what remains after the backend/infra/tooling are complete.

## Not yet built

- **Further microservice extraction** — the Notification service is extracted (RabbitMQ consumer over
  the outbox/integration events); next pull Analytics or Billing out of the monolith
  (`docs/IMPROVEMENTS.md §1`).
- **Run Playwright E2E against a live stack** — specs exist (`frontend/e2e`: auth, dashboard,
  reservation lifecycle, public booking); wire them into CI with the backend + web server running.
- **Frontend depth** — the Next.js app covers the public marketing/booking site (SSG/ISR/SEO), BFF
  auth, the executive dashboard and entity modules, with server actions for reservations
  (create/cancel/invoice) and tenant-feature toggles. Still to add: edit forms for the remaining
  entities and server-side pagination for large datasets.

## Nice-to-have hardening

- Real OTLP/tracing backend (Tempo/Jaeger) wired in Compose for end-to-end traces.
- Swap MassTransit in-memory transport for RabbitMQ or Amazon SQS.
- ACM certificate + HTTPS listener + Route 53 record in the Terraform stack.
- Remote Terraform backend (S3 + DynamoDB lock) before any team use.
- MongoDB target for production (DocumentDB or Atlas) — currently optional with a no-op fallback.

## Future direction

See [`docs/IMPROVEMENTS.md`](docs/IMPROVEMENTS.md) for the full enhancement analysis
(event-driven microservices, gRPC, semantic search / RAG, Kubernetes, DevSecOps, cloud networking).
