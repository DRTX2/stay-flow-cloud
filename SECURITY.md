# Security Policy

## Reporting

Do not open public issues for suspected vulnerabilities. Report privately to the repository owner with reproduction steps, impacted versions and any known mitigations.

## Baseline Controls

- Secrets must live in GitHub Actions secrets, Azure managed configuration or local `.env` files that are ignored by git.
- Pull requests run build, tests, dependency review, Trivy and CodeQL.
- Container images run as non-root users and are rebuilt through CI/CD.
- Production deployments should use GitHub OIDC for Azure, not long-lived publish profiles or service principal passwords.

## Supported Branches

Only `main` receives security fixes unless a release branch is explicitly created.
