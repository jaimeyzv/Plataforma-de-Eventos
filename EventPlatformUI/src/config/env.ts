export const env = {
  apiUrl: (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, "") ?? "http://localhost:8080",
  apiToken: (import.meta.env.VITE_API_TOKEN as string | undefined) ?? "",
};
