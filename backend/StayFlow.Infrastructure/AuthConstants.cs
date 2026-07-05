namespace StayFlow.Infrastructure;

/// <summary>Well-known identifiers used across the auth stack (scopes, clients, claims).</summary>
public static class AuthConstants
{
    /// <summary>The API scope external clients request to call the platform API.</summary>
    public const string ApiScope = "stayflow.api";

    /// <summary>Custom claim carrying the user's tenant; consumed by the tenant provider.</summary>
    public const string TenantClaim = "tenant_id";

    /// <summary>Custom claim carrying a granted permission string.</summary>
    public const string PermissionClaim = "permission";

    public static class Clients
    {
        /// <summary>Public SPA/first-party client — uses Authorization Code + PKCE only.</summary>
        public const string Spa = "stayflow-spa";

        /// <summary>Confidential machine client using the client-credentials grant.</summary>
        public const string Service = "stayflow-service";

        /// <summary>Development/test-only machine client with elevated permissions for automated tests.</summary>
        public const string TestAdmin = "stayflow-test-admin";

        // ServiceSecret is NO LONGER a compile-time constant.
        // It is read from configuration key "Authentication:ServiceClientSecret"
        // at seeding time, falling back to a generated secret in development only.
        // See DataSeeder.SeedClientsAsync.
    }
}
