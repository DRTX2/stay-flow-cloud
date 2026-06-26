import "server-only";

/** Discriminated result returned by every server action so the UI can toast precisely. */
export interface ActionResult {
  ok: boolean;
  error?: string;
}

export const ok: ActionResult = { ok: true };

/**
 * Build a failure result from a non-2xx API response, surfacing the RFC7807 problem-details
 * message when the backend provides one (e.g. FluentValidation errors) so the user sees the
 * real reason ("Room number '101' already exists.") rather than a bare status code.
 */
export async function fail(res: Response, fallback: string): Promise<ActionResult> {
  let message = `${fallback} (${res.status}).`;
  try {
    const body: unknown = await res.json();
    if (body && typeof body === "object") {
      const pd = body as {
        detail?: string;
        title?: string;
        errors?: Record<string, string[]>;
      };
      const fieldError = pd.errors
        ? Object.values(pd.errors).flat().find(Boolean)
        : undefined;
      message = fieldError ?? pd.detail ?? pd.title ?? message;
    }
  } catch {
    // Non-JSON body (or empty); keep the fallback message.
  }
  return { ok: false, error: message };
}
