# DevSecOps Baseline

## Pipeline

| Stage | Control |
|---|---|
| Validate | .NET restore/build/test, Next.js lint/typecheck/unit/build, Playwright E2E |
| SCA | NuGet audit, `npm audit`, Dependabot, dependency review |
| SAST | CodeQL for C# and TypeScript |
| Secrets/IaC/image scan | Trivy filesystem scan, image scan and IaC review baseline |
| Package | Immutable Docker images tagged with commit SHA |
| Deploy | GitHub OIDC to Azure, ACR admin disabled, managed identity AcrPull |

## Current Hardening Decisions

| Area | Decision |
|---|---|
| Secrets | Runtime secrets are documented in `.env.example`; real values belong in GitHub/Azure secrets, not source. |
| OAuth | Interactive login uses Authorization Code + PKCE. Password / ROPC is not the primary browser flow. |
| Database lifecycle | API startup does not run migrations. `StayFlow.MigrationHost` owns migrate/seed/rollback/status commands. |
| Student deployment | Azure Container Apps + PostgreSQL keeps cost low while preserving containerized deployment. |

## Azure Path

The default Azure deployment is intentionally lean for Azure for Students:

- Azure Container Apps for API and frontend.
- Azure Database for PostgreSQL Flexible Server as the required persistent store.
- Redis, MongoDB and RabbitMQ are optional in the current code path and can be added later.
- Log Analytics captures platform logs.

## Hardening Backlog

- Move PostgreSQL to private networking with Container Apps VNet integration.
- Add custom domains and managed certificates for API/frontend.
- Add environment protection rules and required reviewers for `production`.
- Add SBOM generation and image signing with cosign.
- Add DAST against a deployed staging environment.
- Run `StayFlow.MigrationHost migrate` as an explicit pre-deploy job in the Azure workflow.
- Split app and migrator database users so only the migrator has DDL permissions.
