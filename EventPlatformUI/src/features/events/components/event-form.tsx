import { useState } from "react";
import { useCreateEvent } from "../hooks/useCreateEvent";
import type { ZoneInput } from "../types/event.types";
import { hasErrors, validateEvent, type FormErrors } from "../validation";
import ZoneList from "./zone-list";

const emptyZone = (): ZoneInput => ({ name: "", price: 0, capacity: 1 });
const noErrors: FormErrors = { zoneRows: {} };

const fieldClass =
  "w-full rounded border border-slate-300 px-3 py-2 text-sm focus:border-slate-500 focus:outline-none";

export default function EventForm() {
  const [name, setName] = useState("");
  const [eventDate, setEventDate] = useState("");
  const [place, setPlace] = useState("");
  const [zones, setZones] = useState<ZoneInput[]>([emptyZone()]);
  const [errors, setErrors] = useState<FormErrors>(noErrors);

  const { loading, error, result, submit, reset } = useCreateEvent();

  function updateZone(index: number, field: keyof ZoneInput, value: string) {
    setZones((prev) =>
      prev.map((zone, i) => {
        if (i !== index) return zone;
        if (field === "name") return { ...zone, name: value };
        return { ...zone, [field]: value === "" ? NaN : Number(value) };
      }),
    );
  }

  function addZone() {
    setZones((prev) => [...prev, emptyZone()]);
  }

  function removeZone(index: number) {
    setZones((prev) => prev.filter((_, i) => i !== index));
  }

  function resetForm() {
    setName("");
    setEventDate("");
    setPlace("");
    setZones([emptyZone()]);
    setErrors(noErrors);
    reset();
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const values = { name, eventDate, place, zones };
    const validation = validateEvent(values);
    setErrors(validation);
    if (hasErrors(validation)) return;

    const ok = await submit({
      name: name.trim(),
      // Convert the datetime-local value into an ISO-8601 instant with offset.
      eventDate: new Date(eventDate).toISOString(),
      place: place.trim(),
      zones: zones.map((z) => ({ name: z.name.trim(), price: z.price, capacity: z.capacity })),
    });

    if (ok) {
      setName("");
      setEventDate("");
      setPlace("");
      setZones([emptyZone()]);
      setErrors(noErrors);
    }
  }

  if (result) {
    return (
      <div className="rounded-lg border border-green-200 bg-green-50 p-6">
        <h3 className="text-lg font-semibold text-green-800">¡Evento registrado!</h3>
        <p className="mt-1 text-sm text-green-700">
          <strong>{result.event.name}</strong> se creó con {result.event.zones.length} zona(s).
        </p>
        <dl className="mt-3 space-y-1 text-xs text-green-700">
          <div><dt className="inline font-medium">Event ID: </dt><dd className="inline">{result.event.eventId}</dd></div>
          <div><dt className="inline font-medium">Correlation ID: </dt><dd className="inline">{result.correlationId}</dd></div>
        </dl>
        <button
          type="button"
          onClick={resetForm}
          className="mt-4 rounded bg-green-700 px-4 py-2 text-sm font-medium text-white hover:bg-green-600"
        >
          Registrar otro evento
        </button>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-5 rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <div>
        <label htmlFor="name" className="mb-1 block text-sm font-medium text-slate-700">
          Nombre del evento
        </label>
        <input id="name" className={fieldClass} value={name} onChange={(e) => setName(e.target.value)} />
        {errors.name && <p className="mt-1 text-xs text-red-600">{errors.name}</p>}
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="eventDate" className="mb-1 block text-sm font-medium text-slate-700">
            Fecha
          </label>
          <input
            id="eventDate"
            type="datetime-local"
            className={fieldClass}
            value={eventDate}
            onChange={(e) => setEventDate(e.target.value)}
          />
          {errors.eventDate && <p className="mt-1 text-xs text-red-600">{errors.eventDate}</p>}
        </div>
        <div>
          <label htmlFor="place" className="mb-1 block text-sm font-medium text-slate-700">
            Lugar
          </label>
          <input id="place" className={fieldClass} value={place} onChange={(e) => setPlace(e.target.value)} />
          {errors.place && <p className="mt-1 text-xs text-red-600">{errors.place}</p>}
        </div>
      </div>

      <ZoneList zones={zones} errors={errors} onChange={updateZone} onAdd={addZone} onRemove={removeZone} />

      {error && (
        <div className="rounded border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
          {error}
        </div>
      )}

      <button
        type="submit"
        disabled={loading}
        className="w-full rounded bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
      >
        {loading ? "Guardando…" : "Guardar"}
      </button>
    </form>
  );
}
