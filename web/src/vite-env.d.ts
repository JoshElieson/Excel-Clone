/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Absolute API origin (e.g. https://api.example.com). Omit to use same-origin /api (Vite proxy or Vercel rewrite). */
  readonly VITE_API_URL: string | undefined
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
