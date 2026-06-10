import type { ZoneInput } from "./types/event.types";

export interface FormErrors {
  name?: string;
  eventDate?: string;
  place?: string;
  zones?: string;
  zoneRows: Record<number, { name?: string; price?: string; capacity?: string }>;
}

export interface EventFormValues {
  name: string;
  eventDate: string;
  place: string;
  zones: ZoneInput[];
}

/**
 * Minimal validation required by the challenge: required fields,
 * capacity > 0 and price >= 0.
 */
export function validateEvent(values: EventFormValues): FormErrors {
  const errors: FormErrors = { zoneRows: {} };

  if (!values.name.trim()) errors.name = "El nombre es obligatorio.";
  if (!values.eventDate) errors.eventDate = "La fecha es obligatoria.";
  if (!values.place.trim()) errors.place = "El lugar es obligatorio.";
  if (values.zones.length === 0) errors.zones = "Agrega al menos una zona.";

  values.zones.forEach((zone, index) => {
    const rowErrors: { name?: string; price?: string; capacity?: string } = {};
    if (!zone.name.trim()) rowErrors.name = "Nombre requerido.";
    if (Number.isNaN(zone.price) || zone.price < 0) rowErrors.price = "El precio debe ser ≥ 0.";
    if (Number.isNaN(zone.capacity) || zone.capacity <= 0) rowErrors.capacity = "La capacidad debe ser > 0.";
    if (Object.keys(rowErrors).length > 0) errors.zoneRows[index] = rowErrors;
  });

  return errors;
}

export function hasErrors(errors: FormErrors): boolean {
  return Boolean(
    errors.name ||
      errors.eventDate ||
      errors.place ||
      errors.zones ||
      Object.keys(errors.zoneRows).length > 0,
  );
}
