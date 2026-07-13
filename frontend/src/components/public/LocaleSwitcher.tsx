"use client";

import { Languages } from "lucide-react";
import { Button } from "@/components/ui/button";
import type { Locale } from "@/i18n/config";

export function LocaleSwitcher({ locale }: { locale: Locale }) {
  function changeLocale() {
    const next = locale === "en" ? "es" : "en";
    document.cookie = `sf_locale=${next}; path=/; max-age=31536000; samesite=lax`;
    window.location.reload();
  }

  return (
    <Button
      type="button"
      variant="ghost"
      size="sm"
      onClick={changeLocale}
      aria-label={locale === "en" ? "Cambiar a español" : "Switch to English"}
    >
      <Languages className="mr-1.5 h-4 w-4" />
      {locale === "en" ? "ES" : "EN"}
    </Button>
  );
}
