export interface ZoneInput {
  name: string;
  price: number;
  capacity: number;
}

export interface CreateEventRequest {
  name: string;
  eventDate: string; // ISO-8601
  place: string;
  zones: ZoneInput[];
}

export interface ZoneDto {
  zoneId: string;
  name: string;
  price: number;
  capacity: number;
}

export interface EventDto {
  eventId: string;
  name: string;
  eventDate: string;
  place: string;
  status: string;
  zones: ZoneDto[];
}

export interface CreateEventResponse {
  event: EventDto;
  correlationId: string;
}

export interface GetAllEventsResponse {
  events: EventDto[];
}
