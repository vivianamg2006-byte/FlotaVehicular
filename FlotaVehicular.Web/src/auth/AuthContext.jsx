import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { logout as apiLogout } from "../api/authApi.js";
import { UNAUTHORIZED_EVENT } from "../api/client.js";
import { clearFlash, clearSession, loadSession, saveSession, setFlash } from "./session.js";

const AuthContext = createContext(null);
const MAX_TIMEOUT = 2147483647;
const EXPIRED_MESSAGE = "Tu sesión expiró. Inicia sesión nuevamente para continuar.";

export function AuthProvider({ children }) {
  const [session, setSession] = useState(() => loadSession());

  const applyExpiration = useCallback((message) => {
    if (message) setFlash(message);
    clearSession();
    setSession(null);
  }, []);

  const signIn = useCallback((loginData) => {
    saveSession(loginData);
    setSession(loginData);
  }, []);

  const signOut = useCallback(async () => {
    await apiLogout();
    clearFlash();
    clearSession();
    setSession(null);
  }, []);

  const expire = useCallback(() => {
    applyExpiration(EXPIRED_MESSAGE);
  }, [applyExpiration]);

  useEffect(() => {
    const handleUnauthorized = () => expire();
    window.addEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
    return () => window.removeEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
  }, [expire]);

  useEffect(() => {
    if (!session?.expiresAt) return undefined;

    const remaining = new Date(session.expiresAt).getTime() - Date.now();
    if (!Number.isFinite(remaining) || remaining <= 0) {
      expire();
      return undefined;
    }

    const timeout = setTimeout(expire, Math.min(remaining, MAX_TIMEOUT));
    return () => clearTimeout(timeout);
  }, [session, expire]);

  const value = useMemo(
    () => ({
      session,
      user: session?.user ?? null,
      isAuthenticated: Boolean(session?.token),
      signIn,
      signOut,
      expire,
    }),
    [session, signIn, signOut, expire],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth debe usarse dentro de AuthProvider.");
  return context;
}
