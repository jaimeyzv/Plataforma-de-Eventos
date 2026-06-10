import { NavLink, Navigate, Route, Routes } from "react-router-dom";
import RegisterEventView from "./features/events/views/register-event-view";
import SearchEventsView from "./features/events/views/search-events-view";

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `rounded px-3 py-1.5 text-sm font-medium transition-colors ${
    isActive ? "bg-white text-slate-900" : "text-slate-300 hover:bg-slate-800 hover:text-white"
  }`;

function App() {
  return (
    <div className="min-h-screen">
      <header className="bg-slate-900 text-white">
        <div className="mx-auto flex max-w-3xl flex-wrap items-center justify-between gap-3 px-4 py-4">
          <div>
            <h1 className="text-xl font-semibold">Event Platform</h1>
            <p className="text-sm text-slate-300">Plataforma de Eventos Online</p>
          </div>
          <nav className="flex gap-2">
            <NavLink to="/" end className={navLinkClass}>
              Registrar
            </NavLink>
            <NavLink to="/eventos" className={navLinkClass}>
              Buscar
            </NavLink>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-3xl px-4 py-8">
        <Routes>
          <Route path="/" element={<RegisterEventView />} />
          <Route path="/eventos" element={<SearchEventsView />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
