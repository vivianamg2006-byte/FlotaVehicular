import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getCurrentUser } from "../api/authApi.js";
import { useAuth } from "../auth/AuthContext.jsx";

const ROLE_LABELS = {
  Administrador: "Administrador",
  CoordinadorTecnico: "Coordinador/Técnico",
  SolicitanteConsulta: "Solicitante/Consulta",
};

function readableRole(role) {
  return ROLE_LABELS[role] || role || "Sin rol";
}

export default function HomePage() {
  const navigate = useNavigate();
  const { user, signOut } = useAuth();
  const [profile, setProfile] = useState(null);
  const [error, setError] = useState("");
  const [closing, setClosing] = useState(false);

  useEffect(() => {
    let active = true;

    getCurrentUser()
      .then((result) => {
        if (active) setProfile(result?.data?.user ?? result?.data ?? null);
      })
      .catch((caught) => {
        if (active) setError(caught?.message || "No se pudo cargar la sesión.");
      });

    return () => {
      active = false;
    };
  }, []);

  async function handleSignOut() {
    setClosing(true);
    await signOut();
    navigate("/login", { replace: true });
  }

  const current = profile || user || {};
  const fullName = current.fullName || user?.fullName || current.username || user?.username || "";
  const role = readableRole(current.role || user?.role);

  return (
    <main className="home-shell">
      <header className="home-header">
        <div className="brand">
          <svg className="brand-icon" viewBox="0 0 40 40" aria-hidden="true">
            <g fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M5 25h22l7-8h-6l-4 5H13l-4-5H6z" />
              <path d="M15 17l3-6h8l5 6" />
              <circle cx="14" cy="28" r="3.5" />
              <circle cx="29" cy="28" r="3.5" />
            </g>
          </svg>
          <span>Flota Vehicular</span>
        </div>

        <div className="session-summary">
          <span>{fullName} · {role}</span>
          <button className="secondary-button" type="button" disabled={closing} onClick={handleSignOut}>
            {closing ? "Cerrando..." : "Cerrar sesión"}
          </button>
        </div>
      </header>

      <section className="home-content">
        <article className="session-card">
          <h1>Sesión iniciada</h1>
          <p>El login funciona de extremo a extremo. Esta pantalla es provisional.</p>

          {error ? (
            <p className="error-box" role="alert">
              <span aria-hidden="true">⚠</span> {error}
            </p>
          ) : (
            <dl className="session-details">
              <div>
                <dt>Usuario</dt>
                <dd>{current.username || "—"}</dd>
              </div>
              <div>
                <dt>Correo</dt>
                <dd>{current.email || "—"}</dd>
              </div>
              <div>
                <dt>Rol</dt>
                <dd>{role}</dd>
              </div>
            </dl>
          )}
        </article>
      </section>
    </main>
  );
}
