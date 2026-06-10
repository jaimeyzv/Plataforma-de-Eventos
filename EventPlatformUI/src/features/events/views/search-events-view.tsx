import { useMemo, useState } from "react";
import type { EventSearchParams } from "../api/events.api";
import { useDebounce } from "../hooks/useDebounce";
import { useEvents } from "../hooks/useEvents";
import type { EventDto } from "../types/event.types";

const PUBLISHED = "Published";

interface Filters {
  text: string; // nombre o lugar
  from: string; // fecha desde (yyyy-mm-dd)
  to: string; // fecha hasta (yyyy-mm-dd)
  maxPrice: string; // precio máximo de entrada
}

const emptyFilters: Filters = { text: "", from: "", to: "", maxPrice: "" };

function formatDate(iso: string): string {
  const date = new Date(iso);
  return Number.isNaN(date.getTime())
    ? iso
    : date.toLocaleString("es", { dateStyle: "medium", timeStyle: "short" });
}

function priceRange(event: EventDto): string {
  if (event.zones.length === 0) return "—";
  const prices = event.zones.map((z) => z.price);
  const min = Math.min(...prices);
  const max = Math.max(...prices);
  return min === max ? `$${min}` : `$${min} – $${max}`;
}

const inputClass =
  "w-full rounded border border-slate-300 px-3 py-2 text-sm focus:border-slate-500 focus:outline-none";
const labelClass = "mb-1 block text-xs font-medium text-slate-600";

export default function SearchEventsView() {
  const [filters, setFilters] = useState<Filters>(emptyFilters);

  // Debounce so cada tecla no dispara una petición; la búsqueda viaja al backend.
  const debounced = useDebounce(filters, 350);

  // Criterios enviados al backend. Siempre acotado a eventos publicados.
  const params = useMemo<EventSearchParams>(() => {
    const maxPrice = debounced.maxPrice.trim();
    return {
      status: PUBLISHED,
      text: debounced.text.trim() || undefined,
      from: debounced.from ? `${debounced.from}T00:00:00` : undefined,
      to: debounced.to ? `${debounced.to}T23:59:59` : undefined,
      maxPrice: maxPrice === "" ? undefined : Number(maxPrice),
    };
  }, [debounced]);

  const { events, loading, error, refresh } = useEvents(params);

  const update = (field: keyof Filters, value: string) =>
    setFilters((prev) => ({ ...prev, [field]: value }));

  const hasFilters = Object.values(filters).some((v) => v.trim() !== "");

  return (
    <section>
      <div className="mb-6 flex items-start justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-slate-800">Buscar Eventos</h2>
          <p className="text-sm text-slate-500">
            Búsqueda avanzada de eventos <strong>publicados</strong>. Los criterios se aplican en
            el backend (SQL) y la respuesta se cachea en Redis.
          </p>
        </div>
        <button
          type="button"
          onClick={() => void refresh()}
          className="shrink-0 rounded border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100"
        >
          Actualizar
        </button>
      </div>

      {/* Criterios de búsqueda */}
      <div className="mb-4 rounded border border-slate-200 bg-slate-50 p-4">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <div className="sm:col-span-2">
            <label className={labelClass} htmlFor="f-text">
              Nombre o lugar
            </label>
            <input
              id="f-text"
              type="search"
              className={inputClass}
              placeholder="Ej. Tech Summit, Atlantic City…"
              value={filters.text}
              onChange={(e) => update("text", e.target.value)}
            />
          </div>
          <div>
            <label className={labelClass} htmlFor="f-from">
              Desde
            </label>
            <input
              id="f-from"
              type="date"
              className={inputClass}
              value={filters.from}
              onChange={(e) => update("from", e.target.value)}
            />
          </div>
          <div>
            <label className={labelClass} htmlFor="f-to">
              Hasta
            </label>
            <input
              id="f-to"
              type="date"
              className={inputClass}
              value={filters.to}
              onChange={(e) => update("to", e.target.value)}
            />
          </div>
          <div>
            <label className={labelClass} htmlFor="f-price">
              Precio máximo
            </label>
            <input
              id="f-price"
              type="number"
              min="0"
              step="0.01"
              className={inputClass}
              placeholder="Sin límite"
              value={filters.maxPrice}
              onChange={(e) => update("maxPrice", e.target.value)}
            />
          </div>
        </div>

        {hasFilters && (
          <div className="mt-3 flex justify-end">
            <button
              type="button"
              onClick={() => setFilters(emptyFilters)}
              className="text-xs font-medium text-slate-500 hover:text-slate-700"
            >
              Limpiar filtros
            </button>
          </div>
        )}
      </div>

      {!loading && !error && (
        <p className="mb-3 text-xs text-slate-500">
          {events.length} resultado{events.length === 1 ? "" : "s"}
        </p>
      )}

      {loading && <p className="text-sm text-slate-500">Buscando…</p>}
      {error && <p className="text-sm text-red-600">{error}</p>}

      {!loading && !error && events.length === 0 && (
        <p className="text-sm text-slate-500">
          No se encontraron eventos publicados para los criterios seleccionados.
        </p>
      )}

      <ul className="space-y-3">
        {events.map((event) => (
          <li
            key={event.eventId}
            className="rounded border border-slate-200 bg-white p-4 shadow-sm"
          >
            <div className="flex items-start justify-between gap-3">
              <div>
                <h3 className="font-semibold text-slate-800">{event.name}</h3>
                <p className="text-sm text-slate-500">
                  {formatDate(event.eventDate)} · {event.place}
                </p>
              </div>
              <span className="shrink-0 rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-medium text-emerald-700">
                {event.status}
              </span>
            </div>
            <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-slate-500">
              <span>
                {event.zones.length} zona{event.zones.length === 1 ? "" : "s"}
              </span>
              <span>Precio: {priceRange(event)}</span>
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
}
