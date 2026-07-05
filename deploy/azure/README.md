# Azure Deployment

This folder contains the Azure baseline used by GitHub Actions. It targets Azure Container Apps to keep the setup practical for Azure for Students while preserving production-style controls.

## Resources

| Concern | Azure resource |
|---|---|
| Containers | Azure Container Apps, consumption plan |
| Registry | Azure Container Registry Basic, admin disabled |
| Database | Azure Database for PostgreSQL Flexible Server, burstable B1ms |
| Logs | Log Analytics workspace |
| Identity | GitHub Actions OIDC + managed identity AcrPull |

## Deploy

Use `.github/workflows/deploy-azure.yml`. Required GitHub repository secrets:

| Secret | Purpose |
|---|---|
| `AZURE_CLIENT_ID` | Federated app registration/client ID for GitHub Actions |
| `AZURE_TENANT_ID` | Microsoft Entra tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AZURE_POSTGRES_ADMIN_PASSWORD` | Strong PostgreSQL admin password |

Optional GitHub repository variables:

| Variable | Default |
|---|---|
| `AZURE_RESOURCE_GROUP` | `rg-stayflow-dev` |
| `AZURE_LOCATION` | `eastus` |
| `AZURE_ENVIRONMENT_NAME` | `stayflow-dev` |

The workflow provisions infrastructure with `main.bicep`, reads the generated API/frontend hostnames, builds images with the correct Next.js public URLs, pushes them to ACR, then deploys the Container Apps with `apps.bicep` and immutable image tags.

## Cost Notes

Defaults are intentionally small for Azure for Students. Delete the resource group when not using it:

```bash
az group delete --name rg-stayflow-dev --yes
```

For a real production environment, move PostgreSQL behind private networking, use custom domains/TLS, set `minReplicas` to at least `1`, and add Redis/RabbitMQ/Mongo managed services as needed.
