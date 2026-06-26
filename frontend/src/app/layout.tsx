import type { Metadata, Viewport } from "next";
import { Inter } from "next/font/google";
import { Providers } from "./providers";
import "./globals.css";

const inter = Inter({
  subsets: ["latin"],
  variable: "--font-sans",
  display: "swap",
});

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    default: "StayFlow Cloud — Hospitality Management Platform",
    template: "%s · StayFlow Cloud",
  },
  description:
    "StayFlow Cloud is a multi-tenant hospitality management platform: reservations, " +
    "front desk, billing and analytics for modern hotels.",
  applicationName: "StayFlow Cloud",
  authors: [{ name: "StayFlow Cloud" }],
  keywords: [
    "hotel management",
    "PMS",
    "hospitality SaaS",
    "reservations",
    "booking engine",
  ],
  openGraph: {
    type: "website",
    siteName: "StayFlow Cloud",
    title: "StayFlow Cloud — Hospitality Management Platform",
    description: "Reservations, front desk, billing and analytics for modern hotels.",
    url: siteUrl,
  },
  twitter: {
    card: "summary_large_image",
    title: "StayFlow Cloud",
    description: "Reservations, front desk, billing and analytics for modern hotels.",
  },
  robots: { index: true, follow: true },
};

export const viewport: Viewport = {
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#ffffff" },
    { media: "(prefers-color-scheme: dark)", color: "#0a0f1a" },
  ],
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${inter.variable} font-sans`}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
