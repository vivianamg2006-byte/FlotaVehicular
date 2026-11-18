import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { login as apiLogin } from "../api/authApi.js";
import { useAuth } from "../auth/AuthContext.jsx";
import { clearFlash, consumeFlash } from "../auth/session.js";
import FleetScene from "../components/FleetScene.jsx";
import "../styles/login.css";

const TOTAL_LOAD_MS = 2950;
const MIN_VALIDATING_MS = 450;
const WELCOME_HOLD_MS = 900;
const MIN_SCENE_MS = 900;
const REDUCED_MOTION_MS = 300;

const wait = (milliseconds) => new Promise((resolve) => setTimeout(resolve, milliseconds));

function prefersReducedMotion() {
  return typeof window !== "undefined" && Boolean(window.matchMedia?.("(prefers-reduced-motion: reduce)").matches);
}

function firstName(user) {
  const fullName = user?.fullName?.trim();
  if (fullName) return fullName.split(/\s+/)[0];
  return user?.username?.trim() || "";
}

export default function LoginPage() {
  const navigate = useNavigate();
  const { session, isAuthenticated, signIn } = useAuth();
  const [alreadySignedIn] = useState(() => isAuthenticated);
  const [flash] = useState(() => consumeFlash());
  const [step, setStep] = useState(1);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [validating, setValidating] = useState(false);
  const [phase, setPhase] = useState("form");
  const [error, setError] = useState("");

  const usernameRef = useRef(null);
  const passwordRef = useRef(null);
  const sceneRef = useRef(null);
  const fleetTimerRef = useRef(0);
  const welcomeTimerRef = useRef(0);
  const sequenceStartedRef = useRef(false);

  useEffect(
    () => () => {
      if (!sequenceStartedRef.current) return;
      clearTimeout(fleetTimerRef.current);
      clearTimeout(welcomeTimerRef.current);
    },
    [],
  );

  useEffect(() => {
    if (alreadySignedIn) navigate("/", { replace: true });
  }, [navigate, alreadySignedIn]);

  useEffect(() => {
    if (validating || phase !== "form") return;
    const field = step === 1 ? usernameRef.current : passwordRef.current;
    field?.focus();
  }, [step, validating, phase]);

  const welcome = phase === "welcome";

  const stepLabel = welcome
    ? "03 Acceso completado"
    : validating
      ? "Iniciando sesión"
      : step === 1
        ? "01 Usuario"
        : "02 Contraseña";

  function handleUsernameSubmit(event) {
    event.preventDefault();
    const value = username.trim();

    if (!value) {
      setError("Escribe tu usuario o correo para continuar.");
      usernameRef.current?.focus();
      return;
    }

    setUsername(value);
    setError("");
    clearFlash();
    setStep(2);
  }

  function handleChangeUser(event) {
    event.preventDefault();
    if (validating) return;
    setStep(1);
    setPassword("");
    setShowPassword(false);
    setError("");
  }

  async function handlePasswordSubmit(event) {
    event.preventDefault();
    if (validating) return;

    if (!password) {
      setError("Escribe tu contraseña para ingresar.");
      passwordRef.current?.focus();
      return;
    }

    setValidating(true);
    setError("");
    const startedAt = performance.now();

    try {
      const result = await apiLogin(username, password);
      const data = result?.data;
      if (!data?.token) throw new Error("No se pudo iniciar la sesión. Inténtalo de nuevo.");

      signIn(data);

      const elapsed = performance.now() - startedAt;
      if (elapsed < MIN_VALIDATING_MS) await wait(MIN_VALIDATING_MS - elapsed);

      if (sequenceStartedRef.current) return;
      sequenceStartedRef.current = true;

      const reducedMotion = prefersReducedMotion();
      const fleetDuration = reducedMotion
        ? REDUCED_MOTION_MS
        : Math.max(MIN_SCENE_MS, TOTAL_LOAD_MS - (performance.now() - startedAt));
      const welcomeDuration = reducedMotion ? REDUCED_MOTION_MS : WELCOME_HOLD_MS;

      setPhase("fleet");
      sceneRef.current?.start(fleetDuration);

      fleetTimerRef.current = setTimeout(() => {
        setValidating(false);
        setPhase("welcome");

        welcomeTimerRef.current = setTimeout(() => {
          navigate("/", { replace: true });
        }, welcomeDuration);
      }, fleetDuration);
    } catch (caught) {
      const status = caught?.status ?? 0;
      const message =
        status === 401
          ? "Usuario o contraseña incorrectos. Revisa los datos e inténtalo de nuevo."
          : caught?.message || "No se pudo completar la operación.";

      setError(message);
      setPassword("");
      setShowPassword(false);
      setStep(2);
      setValidating(false);
    }
  }

  return (
    <main className="login-shell">
      <div className="login-glow" aria-hidden="true" />

      <section className="login-panel">
        <header className="login-header">
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
          <span className="step-label" aria-live="polite">
            {stepLabel}
          </span>
        </header>

        <FleetScene ref={sceneRef} />

        {flash && phase === "form" && !validating && (
          <p className="notice" role="status">
            <span aria-hidden="true">ⓘ</span> {flash}
          </p>
        )}

        {welcome ? (
          <div className="welcome" role="status">
            <span className="check-mark" aria-hidden="true">
              ✓
            </span>
            <h1>¡Te damos la bienvenida a tu flota!</h1>
            <p>Sesión iniciada · Hola, {firstName(session?.user)}</p>
          </div>
        ) : (
          <form className="login-form" onSubmit={step === 1 ? handleUsernameSubmit : handlePasswordSubmit}>
            {step === 1 ? (
              <>
                <div className="form-copy">
                  <h1>Flota Vehicular</h1>
                  <p>Inicio de sesión</p>
                </div>

                <div className="field">
                  <label htmlFor="usuario">Usuario</label>
                  <input
                    id="usuario"
                    ref={usernameRef}
                    className="text-input"
                    type="text"
                    value={username}
                    placeholder="Usuario o correo"
                    autoComplete="username"
                    autoCapitalize="none"
                    autoCorrect="off"
                    spellCheck={false}
                    onChange={(event) => {
                      setUsername(event.target.value);
                      if (error) setError("");
                    }}
                    aria-invalid={Boolean(error)}
                  />
                </div>

                {error && (
                  <p className="error-box" role="alert">
                    <span aria-hidden="true">⚠</span> {error}
                  </p>
                )}

                <button className="primary-button" type="submit">
                  Continuar →
                </button>
                <p className="hint">Accede a la gestión de tu flota</p>
              </>
            ) : (
              <>
                <div className="form-copy">
                  <h1>Hola, {username}</h1>
                  <p>Un paso más para entrar</p>
                </div>

                <div className="field">
                  <label htmlFor="contrasena">Contraseña</label>
                  <div className="password-field">
                    <input
                      id="contrasena"
                      ref={passwordRef}
                      className="text-input"
                      type={showPassword ? "text" : "password"}
                      value={password}
                      placeholder="Contraseña"
                      autoComplete="current-password"
                      autoCapitalize="none"
                      spellCheck={false}
                      disabled={validating}
                      onChange={(event) => {
                        setPassword(event.target.value);
                        if (error) setError("");
                      }}
                      aria-invalid={Boolean(error)}
                    />
                    <button
                      className="toggle-password"
                      type="button"
                      aria-label={showPassword ? "Ocultar contraseña" : "Mostrar contraseña"}
                      aria-pressed={showPassword}
                      disabled={validating}
                      onClick={() => setShowPassword((visible) => !visible)}
                    >
                      {showPassword ? "Ocultar" : "Mostrar"}
                    </button>
                  </div>
                </div>

                {error && (
                  <p className="error-box" role="alert">
                    <span aria-hidden="true">⚠</span> {error}
                  </p>
                )}

                <div className="actions">
                  <button className="primary-button" type="submit" disabled={validating} aria-busy={validating}>
                    {validating ? (
                      <>
                        <span className="spinner" aria-hidden="true" /> Validando...
                      </>
                    ) : (
                      "Ingresar"
                    )}
                  </button>
                  <a
                    className="link-button"
                    href="#cambiar-usuario"
                    aria-disabled={validating}
                    onClick={handleChangeUser}
                  >
                    ← Cambiar usuario
                  </a>
                </div>
              </>
            )}

            <div className="step-dots" aria-hidden="true">
              <span className={phase === "form" && step === 1 && !validating ? "dot active" : "dot"} />
              <span className={phase !== "form" || step === 2 || validating ? "dot active" : "dot"} />
            </div>
          </form>
        )}
      </section>
    </main>
  );
}
