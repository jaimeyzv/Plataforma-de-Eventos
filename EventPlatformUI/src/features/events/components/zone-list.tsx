import type { ZoneInput } from "../types/event.types";
import type { FormErrors } from "../validation";

interface Props {
  zones: ZoneInput[];
  errors: FormErrors;
  onChange: (index: number, field: keyof ZoneInput, value: string) => void;
  onAdd: () => void;
  onRemove: (index: number) => void;
}

const inputClass =
  "w-full rounded border border-slate-300 px-2 py-1 text-sm focus:border-slate-500 focus:outline-none";

export default function ZoneList({ zones, errors, onChange, onAdd, onRemove }: Props) {
  return (
    <fieldset className="space-y-3">
      <div className="flex items-center justify-between">
        <legend className="text-sm font-medium text-slate-700">Zonas</legend>
        <button
          type="button"
          onClick={onAdd}
          className="rounded bg-slate-800 px-3 py-1 text-xs font-medium text-white hover:bg-slate-700"
        >
          + Agregar zona
        </button>
      </div>

      {errors.zones && <p className="text-xs text-red-600">{errors.zones}</p>}

      {zones.length > 0 && (
        <div className="grid grid-cols-12 gap-2 px-2 text-xs font-medium text-slate-500">
          <div className="col-span-5">Nombre</div>
          <div className="col-span-3">Precio</div>
          <div className="col-span-3">Capacidad</div>
          <div className="col-span-1" aria-hidden="true" />
        </div>
      )}

      <div className="space-y-2">
        {zones.map((zone, index) => {
          const rowError = errors.zoneRows[index];
          return (
            <div key={index} className="grid grid-cols-12 gap-2 rounded border border-slate-200 bg-slate-50 p-2">
              <div className="col-span-5">
                <input
                  className={inputClass}
                  placeholder="Nombre de la zona"
                  value={zone.name}
                  onChange={(e) => onChange(index, "name", e.target.value)}
                  aria-label={`Nombre zona ${index + 1}`}
                />
                {rowError?.name && <p className="mt-1 text-xs text-red-600">{rowError.name}</p>}
              </div>
              <div className="col-span-3">
                <input
                  className={inputClass}
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="Precio"
                  value={Number.isNaN(zone.price) ? "" : zone.price}
                  onChange={(e) => onChange(index, "price", e.target.value)}
                  aria-label={`Precio zona ${index + 1}`}
                />
                {rowError?.price && <p className="mt-1 text-xs text-red-600">{rowError.price}</p>}
              </div>
              <div className="col-span-3">
                <input
                  className={inputClass}
                  type="number"
                  min="1"
                  placeholder="Capacidad"
                  value={Number.isNaN(zone.capacity) ? "" : zone.capacity}
                  onChange={(e) => onChange(index, "capacity", e.target.value)}
                  aria-label={`Capacidad zona ${index + 1}`}
                />
                {rowError?.capacity && <p className="mt-1 text-xs text-red-600">{rowError.capacity}</p>}
              </div>
              <div className="col-span-1 flex items-start justify-end">
                <button
                  type="button"
                  onClick={() => onRemove(index)}
                  className="rounded px-2 py-1 text-sm text-red-600 hover:bg-red-50"
                  aria-label={`Eliminar zona ${index + 1}`}
                  title="Eliminar zona"
                >
                  ✕
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </fieldset>
  );
}
