# StayFlow Cloud — AWS Infrastructure (Terraform)

Illustrative, production-shaped IaC for deploying the API to AWS. It provisions:

| Concern | Resource |
|---|---|
| Networking | VPC, 2 public + 2 private subnets, IGW, NAT gateway, route tables |
| Compute | ECS Fargate cluster, task definition, service behind an Application Load Balancer |
| Database | RDS PostgreSQL (private subnets, encrypted, multi-AZ in `prod`) |
| Cache | ElastiCache Redis (replication group, private) |
| Documents | S3 bucket (private, versioned, encrypted) + CloudFront (OAC) |
| Secrets | Secrets Manager (connection strings injected into the task) |
| Security | Least-privilege security groups; task/execution IAM roles |
| Logs/metrics | CloudWatch log group; ECS Container Insights |

## Layout

```
versions.tf     providers + required versions
variables.tf    inputs (region, image, sizing…)
network.tf      VPC / subnets / NAT / routes
security.tf     security groups (alb → api → data)
database.tf     RDS PostgreSQL + ElastiCache Redis
storage.tf      S3 documents bucket + CloudFront
secrets.tf      Secrets Manager (connection strings)
iam.tf          ECS execution + task roles
compute.tf      ECS cluster / ALB / task def / service
outputs.tf      URLs and endpoints
```

## Usage

```bash
cd deploy/terraform
terraform init
terraform plan  -var "container_image=ghcr.io/<owner>/stayflowcloud-api:latest"
terraform apply -var "container_image=ghcr.io/<owner>/stayflowcloud-api:latest"
```

After `apply`, `terraform output api_url` gives the load-balancer URL. The API reads
`ConnectionStrings__Default` and `ConnectionStrings__Redis` from Secrets Manager and the S3
document bucket/region from environment variables.

## Notes

- Configure a remote backend (S3 + DynamoDB lock) in `versions.tf` before using in a team.
- Add an ACM certificate + HTTPS listener and a Route 53 record for a real domain.
- MongoDB (audit store) is optional; point `ConnectionStrings__Mongo` at DocumentDB or Atlas to
  enable it, otherwise the API uses its no-op audit fallback.
- This is reference IaC for the portfolio; review sizing, backups and deletion protection before
  any real deployment.
