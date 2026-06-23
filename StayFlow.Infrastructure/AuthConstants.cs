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
        /// <summary>Public SPA/first-party client using the password (ROPC) grant.</summary>
        public const string Spa = "stayflow-spa";

        /// <summary>Confidential machine client using the client-credentials grant.</summary>
        public const string Service = "stayflow-service";

        /// <summary>Development secret for the confidential service client.</summary>
        public const string ServiceSecret = "stayflow-service-secret";
    }
}
