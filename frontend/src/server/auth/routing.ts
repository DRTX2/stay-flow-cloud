export function isCustomer(roles: readonly string[]): boolean {
  return roles.includes("Customer");
}

/** Keep an allowed return path, but never send a customer into staff pages or staff into the portal. */
export function authenticatedDestination(path: string, roles: readonly string[]): string {
  const safePath = path.startsWith("/") && !path.startsWith("//") ? path : "/";
  if (isCustomer(roles)) {
    return safePath.startsWith("/portal") ? safePath : "/portal";
  }
  return safePath.startsWith("/dashboard") ? safePath : "/dashboard";
}
