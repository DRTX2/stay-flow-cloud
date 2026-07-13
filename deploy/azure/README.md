# Azure Deployment

This folder contains the Azure baseline used by GitHub Actions. It targets Azure Container Apps to keep the setup practical for Azure for Students while preserving production-style controls.

## Resources

| Concern | Azure resource |
|---|---|
| Containers | Azure Container Apps, consumption plan |
| Registry | GitHub Container Registry (`ghcr.io`) |
| Database | Azure Database for PostgreSQL Flexible Server, burstable B1ms |
| Logs | Log Analytics workspace |
| Identity | GitHub Actions OIDC for Azure + GHCR token for private image pulls |

## Deploy

Use `.github/workflows/deploy-azure.yml`. Required GitHub repository secrets:

| Secret | Purpose |
|---|---|
| `AZURE_CLIENT_ID` | Federated app registration/client ID for GitHub Actions |
| `AZURE_TENANT_ID` | Microsoft Entra tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `NEON_CONNECTION_STRING` | PostgreSQL connection string used by the apps and migration job |
| `STAYFLOW_ADMIN_EMAIL` | Initial platform administrator email |
| `STAYFLOW_ADMIN_PASSWORD` | Initial platform administrator password |
| `STAYFLOW_SERVICE_CLIENT_SECRET` | Seeded service-to-service OAuth client secret |
| `GHCR_READ_TOKEN` | GitHub token/PAT with `read:packages` for private GHCR packages |

Optional social sign-in secrets (a provider stays disabled unless both values are present):

| Provider | GitHub environment secrets | Production callback |
|---|---|---|
| Google | `GOOGLE_OAUTH_CLIENT_ID`, `GOOGLE_OAUTH_CLIENT_SECRET` | `${AZURE_API_URL}/signin-google` |
| Microsoft | `MICROSOFT_OAUTH_CLIENT_ID`, `MICROSOFT_OAUTH_CLIENT_SECRET` | `${AZURE_API_URL}/signin-microsoft` |
| Facebook | `FACEBOOK_APP_ID`, `FACEBOOK_APP_SECRET` | `${AZURE_API_URL}/signin-facebook` |

Optional GitHub repository variables:

| Variable | Default |
|---|---|
| `AZURE_RESOURCE_GROUP` | `rg-stayflow-dev` |
| `AZURE_INFRA_LOCATION` | `southcentralus` |
| `AZURE_APP_LOCATION` | `westus3` |
| `AZURE_ENVIRONMENT_NAME` | `stayflow-dev` |
| `AZURE_API_URL` | Public API/OIDC URL |
| `AZURE_WEB_URL` | Public frontend URL |
| `GHCR_USERNAME` | GitHub user/bot for private GHCR pulls; defaults to workflow actor |
| `GHCR_AUTH_ENABLED` | `true`; set `false` only when GHCR packages are public |

The workflow provisions Log Analytics and an Azure Container Apps environment with `main.bicep`, builds images with the correct Next.js public URLs, scans them, publishes immutable commit-SHA tags to GHCR, then deploys Container Apps with `apps.bicep`.

To avoid Azure Container Registry charges, this baseline does not create ACR. If packages are public in GHCR, set `GHCR_AUTH_ENABLED=false`. For private packages, keep it `true` and configure `GHCR_READ_TOKEN`.

## Cost Notes

Defaults are intentionally small for Azure for Students. Delete the resource group when not using it:

```bash
az group delete --name rg-stayflow-dev --yes
```

For a real production environment, move PostgreSQL behind private networking, use custom domains/TLS, set `minReplicas` to at least `1`, and add Redis/RabbitMQ/Mongo managed services as needed.
