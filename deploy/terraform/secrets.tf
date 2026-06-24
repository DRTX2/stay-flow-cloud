# Connection strings the API reads from Secrets Manager (injected as env at task start).
resource "aws_secretsmanager_secret" "app" {
  name = "${local.name}/app"
  tags = { Name = "${local.name}-app-secrets" }
}

resource "aws_secretsmanager_secret_version" "app" {
  secret_id = aws_secretsmanager_secret.app.id
  secret_string = jsonencode({
    "ConnectionStrings__Default" = "Host=${aws_db_instance.postgres.address};Port=5432;Database=stayflow;Username=stayflow;Password=${random_password.db.result}"
    "ConnectionStrings__Redis"   = "${aws_elasticache_replication_group.redis.primary_endpoint_address}:6379"
  })
}
