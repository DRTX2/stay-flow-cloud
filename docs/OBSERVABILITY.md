# Observability

StayFlow uses OpenTelemetry and the three pillars of observability: metrics, traces and structured
logs. The complete stack is local and open source, so it can be demonstrated without a paid cloud
account.

## Local stack

```bash
docker compose up -d --build
bash deploy/validate-observability.sh
```

| Surface | URL | Purpose |
|---|---|---|
| Grafana | `http://localhost:3001` | Dashboards and correlation (`admin` / `admin` locally) |
| Prometheus | `http://localhost:9090` | Metrics, targets and recording rules |
| Alertmanager | `http://localhost:9093` | Local alert routing |
| Tempo | `http://localhost:3200` | Distributed traces |
| Loki | `http://localhost:3100` | Structured container logs |
| Scalar | `http://localhost:8080/docs` | Interactive API documentation |

Grafana provisions four dashboards:

- **StayFlow Service Health & SLO**: availability, 5xx ratio, p95 and throughput.
- **StayFlow API**: RED metrics, runtime, outbox and job latency.
- **StayFlow Jobs, Messaging & Notifications**: outbox age, jobs, events and delivery outcomes.
- **StayFlow Logs & Trace Correlation**: searchable logs with trace links.

## Initial objectives

| SLI | Objective | Window |
|---|---:|---:|
| Eligible API request availability | 99.9% | 30 days |
| p95 API latency | under 500 ms | 30 days |
| Outbox delivery age | under 5 minutes | rolling |
| In-app notification creation | 99% successful | 30 days |

Health, metrics and documentation endpoints are excluded from API availability calculations.
Production currently scales to zero for student-budget control, so cold-start latency is tracked but
is not treated as a strict latency SLO until `minReplicas` is raised.

## Security and cardinality

- Production `/metrics` requires `METRICS_BEARER_TOKEN`.
- Metrics never use tenant, user, reservation, guest or event IDs as labels.
- Logs do not include notification recipients, prompt content or guest data.
- API errors include a trace ID for support correlation without exposing exception details.
- Scalar is enabled explicitly with `Docs__Enabled`; protected operations still require OAuth.

## Production

Azure Container Apps sends container logs to its existing Log Analytics environment. The repository
does not claim a stateful Prometheus/Loki/Tempo deployment in Azure. Applications can export OTLP to
an operator-managed or free-tier endpoint by setting `OpenTelemetry__OtlpEndpoint` in a future
deployment, while the local stack remains the reproducible portfolio and development environment.

## Validation

`deploy/validate-observability.sh` validates Prometheus configuration and rules, the OpenTelemetry
Collector, Alertmanager, every dashboard JSON file and the Compose model. CI should run this script
before building observability images.
