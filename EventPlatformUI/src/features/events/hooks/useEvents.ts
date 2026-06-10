import { useCallback, useEffect, useState } from "react";
import { getEvents, type EventSearchParams } from "../api/events.api";
import type { EventDto } from "../types/event.types";

interface State {
  loading: boolean;
  error: string | null;
  events: EventDto[];
}

/** Fetches events from the backend whenever the search params change. */
export function useEvents(params: EventSearchParams) {
  const [state, setState] = useState<State>({ loading: true, error: null, events: [] });

  // Serialize params so the effect only re-runs when the actual criteria change.
  const paramsKey = JSON.stringify(params);

  const load = useCallback(async () => {
    setState((prev) => ({ ...prev, loading: true, error: null }));
    try {
      const events = await getEvents(JSON.parse(paramsKey) as EventSearchParams);
      setState({ loading: false, error: null, events });
    } catch (err) {
      const message = err instanceof Error ? err.message : "Error inesperado.";
      setState({ loading: false, error: message, events: [] });
    }
  }, [paramsKey]);

  useEffect(() => {
    void load();
  }, [load]);

  return { ...state, refresh: load };
}
