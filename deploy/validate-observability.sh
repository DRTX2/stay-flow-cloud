#!/usr/bin/env sh
set -eu

root_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)

docker run --rm \
  -v "$root_dir/deploy:/etc/prometheus:ro" \
  --entrypoint /bin/promtool \
  prom/prometheus:v3.5.0 \
  check config /etc/prometheus/prometheus.yml

docker run --rm \
  -v "$root_dir/deploy:/etc/prometheus:ro" \
  --entrypoint /bin/promtool \
  prom/prometheus:v3.5.0 \
  check rules /etc/prometheus/prometheus-rules.yml

docker run --rm \
  -v "$root_dir/deploy/otel-collector.yaml:/etc/otelcol-contrib/config.yaml:ro" \
  otel/opentelemetry-collector-contrib:0.128.0 \
  validate --config=/etc/otelcol-contrib/config.yaml

docker run --rm \
  -v "$root_dir/deploy/alertmanager.yml:/etc/alertmanager/alertmanager.yml:ro" \
  --entrypoint /bin/amtool \
  prom/alertmanager:v0.28.1 \
  check-config /etc/alertmanager/alertmanager.yml

for dashboard in "$root_dir"/deploy/grafana/dashboards/*.json; do
  jq empty "$dashboard"
done

docker compose -f "$root_dir/compose.yaml" config --quiet
