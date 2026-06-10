import { env } from "../../../config/env";
import type {
  CreateEventRequest,
  CreateEventResponse,
  EventDto,
  GetAllEventsResponse,
} from "../types/event.types";

let cachedToken: string | null = null;

/**
 * Returns a JWT to call protected endpoints. Uses the fixed token from the
 * environment if provided, otherwise requests a demo token from the API.
 */
async function getToken(): Promise<string> {
  if (env.apiToken) return env.apiToken;
  if (cachedToken) return cachedToken;

  const response = await fetch(`${env.apiUrl}/api/auth/token`, { method: "POST" });
  if (!response.ok) {
    throw new Error("No se pudo obtener el token de autenticación.");
  }
  const data = await response.json();
  cachedToken = data.access_token as string;
  return cachedToken;
}

/** Extracts a readable message from the API's problem-details error body. */
async function readError(response: Response): Promise<string> {
  try {
    const body = await response.json();
    if (Array.isArray(body?.errors) && body.errors.length > 0) return body.errors.join(" ");
    if (typeof body?.title === "string") return body.title;
  } catch {
    /* ignore non-JSON bodies */
  }
  return `Error ${response.status} al guardar el evento.`;
}

export async function createEvent(payload: CreateEventRequest): Promise<CreateEventResponse> {
  const token = await getToken();

  const response = await fetch(`${env.apiUrl}/api/events`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new Error(await readError(response));
  }

  return (await response.json()) as CreateEventResponse;
}

export interface EventSearchParams {
  text?: string;
  from?: string; // ISO-8601 (inclusive)
  to?: string; // ISO-8601 (inclusive)
  maxPrice?: number;
  status?: string;
}

/**
 * Advanced search of events. Filters are sent as query params and applied
 * server-side (SQL), with the response cached in Redis by the backend.
 */
export async function getEvents(params: EventSearchParams = {}): Promise<EventDto[]> {
  const qs = new URLSearchParams();
  if (params.text?.trim()) qs.set("text", params.text.trim());
  if (params.from) qs.set("from", params.from);
  if (params.to) qs.set("to", params.to);
  if (params.maxPrice != null && !Number.isNaN(params.maxPrice)) {
    qs.set("maxPrice", String(params.maxPrice));
  }
  if (params.status) qs.set("status", params.status);

  const query = qs.toString();
  const response = await fetch(`${env.apiUrl}/api/events${query ? `?${query}` : ""}`);

  if (!response.ok) {
    throw new Error(`Error ${response.status} al obtener los eventos.`);
  }

  const data = (await response.json()) as GetAllEventsResponse;
  return data.events ?? [];
}
