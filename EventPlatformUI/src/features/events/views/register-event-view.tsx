import EventForm from "../components/event-form";

export default function RegisterEventView() {
  return (
    <section>
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-slate-800">Registrar Evento</h2>
        <p className="text-sm text-slate-500">
          Completa los datos del evento y sus zonas. Al guardar se publica el evento y se
          notifica de forma asíncrona a los demás microservicios.
        </p>
      </div>
      <EventForm />
    </section>
  );
}
