# Pending work

Snapshot of what remains after the backend/infra/tooling are complete.

## Not yet built

- **Playwright E2E** — frontend now exists (`frontend/`), so the E2E suite can be added next.
- **Frontend depth** — the SPA covers auth + read views (dashboard, reservations, rooms, guests,
  services, reports/CSV). Create/edit forms, the customer portal and pagination are still to add.

## Nice-to-have hardening

- Real OTLP/tracing backend (Tempo/Jaeger) wired in Compose for end-to-end traces.
- Swap MassTransit in-memory transport for RabbitMQ or Amazon SQS.
- ACM certificate + HTTPS listener + Route 53 record in the Terraform stack.
- Remote Terraform backend (S3 + DynamoDB lock) before any team use.
- MongoDB target for production (DocumentDB or Atlas) — currently optional with a no-op fallback.

## Future direction

See [`docs/IMPROVEMENTS.md`](docs/IMPROVEMENTS.md) for the full enhancement analysis
(event-driven microservices, gRPC, semantic search / RAG, Kubernetes, DevSecOps, cloud networking).
