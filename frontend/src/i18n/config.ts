export const locales = ["en", "es"] as const;
export type Locale = (typeof locales)[number];

export const dictionaries = {
  en: {
    common: {
      hotels: "Hotels",
      pricing: "Pricing",
      dashboard: "Dashboard",
      signIn: "Sign in",
      home: "Home",
      skip: "Skip to main content",
      tagline: "Hospitality management for modern hotels.",
    },
    auth: {
      welcome: "Welcome back",
      subtitle: "Sign in to your StayFlow Cloud workspace",
      email: "Email address",
      password: "Password",
      submit: "Sign in",
      or: "or continue with",
      back: "Back to home",
      invalid: "Invalid email or password. Please try again.",
      failed: "Sign-in failed. Please try again.",
    },
    home: {
      eyebrow: "Modern hotel operating system",
      title: "Run every stay from booking to checkout.",
      description:
        "StayFlow Cloud connects public booking, reservations, front desk, housekeeping, billing and guest self-service in one production-ready operating layer for hotels.",
      booking: "Start a booking",
      operator: "Open operator dashboard",
      live: "Live operations",
      today: "Today at Ocean Vista",
      stable: "Stable",
      workflowsTitle: "Three workflows that prove the product.",
      workflowsBody:
        "The cloud architecture, security and automation exist to make these hotel workflows reliable, not distract from them.",
      operators: "Built for operators",
      control: "Less admin drag. More stay control.",
      featured: "Featured stays",
      featuredBody: "A glimpse of the properties powered by StayFlow Cloud.",
      allHotels: "All hotels",
      night: "night",
    },
  },
  es: {
    common: {
      hotels: "Hoteles",
      pricing: "Precios",
      dashboard: "Panel",
      signIn: "Iniciar sesión",
      home: "Inicio",
      skip: "Saltar al contenido principal",
      tagline: "Gestión hotelera para establecimientos modernos.",
    },
    auth: {
      welcome: "Bienvenido de nuevo",
      subtitle: "Inicia sesión en tu espacio de StayFlow Cloud",
      email: "Correo electrónico",
      password: "Contraseña",
      submit: "Iniciar sesión",
      or: "o continúa con",
      back: "Volver al inicio",
      invalid: "Correo o contraseña incorrectos. Inténtalo nuevamente.",
      failed: "No se pudo iniciar sesión. Inténtalo nuevamente.",
    },
    home: {
      eyebrow: "Sistema operativo hotelero moderno",
      title: "Gestiona cada estancia desde la reserva hasta el checkout.",
      description:
        "StayFlow Cloud conecta reservas públicas, recepción, housekeeping, facturación y autoservicio del huésped en una plataforma lista para operar hoteles.",
      booking: "Comenzar una reserva",
      operator: "Abrir panel operativo",
      live: "Operación en vivo",
      today: "Hoy en Ocean Vista",
      stable: "Estable",
      workflowsTitle: "Tres flujos que demuestran el producto.",
      workflowsBody:
        "La arquitectura cloud, seguridad y automatización existen para hacer confiables estos flujos hoteleros, no para distraer de ellos.",
      operators: "Creado para operadores",
      control: "Menos carga administrativa. Más control de cada estancia.",
      featured: "Estancias destacadas",
      featuredBody: "Una muestra de las propiedades gestionadas con StayFlow Cloud.",
      allHotels: "Todos los hoteles",
      night: "noche",
    },
  },
} as const;

export function normalizeLocale(value?: string | null): Locale {
  return value === "es" ? "es" : "en";
}
