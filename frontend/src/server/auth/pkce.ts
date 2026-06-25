/**
 * PKCE + state helpers using Web Crypto, so they run on both the Node and Edge runtimes.
 * The verifier and state are generated on the server and stashed in short-lived httpOnly cookies;
 * the browser never sees them. This is the BFF variant of the Authorization Code + PKCE flow.
 */

function base64UrlEncode(bytes: Uint8Array): string {
  let binary = "";
  for (const b of bytes) binary += String.fromCharCode(b);
  return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

function randomBytes(length: number): Uint8Array {
  const bytes = new Uint8Array(length);
  crypto.getRandomValues(bytes);
  return bytes;
}

export function createCodeVerifier(): string {
  return base64UrlEncode(randomBytes(48));
}

export function createState(): string {
  return base64UrlEncode(randomBytes(16));
}

export async function deriveCodeChallenge(verifier: string): Promise<string> {
  const data = new TextEncoder().encode(verifier);
  const digest = await crypto.subtle.digest("SHA-256", data);
  return base64UrlEncode(new Uint8Array(digest));
}
