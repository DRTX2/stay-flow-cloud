variable "region" {
  description = "AWS region to deploy into."
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Deployment environment name (used in resource names/tags)."
  type        = string
  default     = "prod"
}

variable "name_prefix" {
  description = "Prefix for all named resources."
  type        = string
  default     = "stayflow"
}

variable "vpc_cidr" {
  description = "CIDR block for the VPC."
  type        = string
  default     = "10.20.0.0/16"
}

variable "container_image" {
  description = "Fully-qualified API container image (e.g. ghcr.io/owner/stayflowcloud-api:latest or an ECR URI)."
  type        = string
}

variable "api_desired_count" {
  description = "Number of API tasks to run."
  type        = number
  default     = 2
}

variable "api_cpu" {
  description = "Fargate task CPU units."
  type        = number
  default     = 512
}

variable "api_memory" {
  description = "Fargate task memory (MiB)."
  type        = number
  default     = 1024
}

variable "db_instance_class" {
  description = "RDS instance class."
  type        = string
  default     = "db.t4g.micro"
}

variable "redis_node_type" {
  description = "ElastiCache node type."
  type        = string
  default     = "cache.t4g.micro"
}
