output "api_url" {
  description = "Public URL of the API behind the load balancer."
  value       = "http://${aws_lb.main.dns_name}"
}

output "documents_bucket" {
  description = "S3 bucket holding tenant documents."
  value       = aws_s3_bucket.documents.bucket
}

output "documents_cdn_domain" {
  description = "CloudFront domain serving documents."
  value       = aws_cloudfront_distribution.documents.domain_name
}

output "db_endpoint" {
  description = "RDS PostgreSQL endpoint."
  value       = aws_db_instance.postgres.address
}

output "redis_endpoint" {
  description = "ElastiCache Redis primary endpoint."
  value       = aws_elasticache_replication_group.redis.primary_endpoint_address
}

output "app_secret_arn" {
  description = "Secrets Manager secret holding the app connection strings."
  value       = aws_secretsmanager_secret.app.arn
}
