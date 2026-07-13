import "server-only";
import { cookies } from "next/headers";
import { dictionaries, normalizeLocale } from "./config";

export async function getLocale() {
  return normalizeLocale((await cookies()).get("sf_locale")?.value);
}

export async function getDictionary() {
  return dictionaries[await getLocale()];
}
