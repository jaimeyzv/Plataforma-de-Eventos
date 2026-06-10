import { useState } from "react";
import { createEvent } from "../api/events.api";
import type { CreateEventRequest, CreateEventResponse } from "../types/event.types";

interface State {
  loading: boolean;
  error: string | null;
  result: CreateEventResponse | null;
}

export function useCreateEvent() {
  const [state, setState] = useState<State>({ loading: false, error: null, result: null });

  async function submit(payload: CreateEventRequest): Promise<boolean> {
    setState({ loading: true, error: null, result: null });
    try {
      const result = await createEvent(payload);
      setState({ loading: false, error: null, result });
      return true;
    } catch (err) {
      const message = err instanceof Error ? err.message : "Error inesperado.";
      setState({ loading: false, error: message, result: null });
      return false;
    }
  }

  function reset() {
    setState({ loading: false, error: null, result: null });
  }

  return { ...state, submit, reset };
}
