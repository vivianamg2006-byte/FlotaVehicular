import { API_URL } from "../config.js";
import { getToken } from "../auth/session.js";

export class ApiError extends Error {
  constructor(status, title, message, fieldErrors = {}) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.title = title;
    this.message = message;
    this.fieldErrors = fieldErrors;
  }
}

export const UNAUTHORIZED_EVENT = "flota:unauthorized";

const MESSAGES = {
  401: "Tu sesión no es válida o expiró. Inicia sesión nuevamente.",
  403: "No tienes permisos para realizar esta acción.",
  404: "No se encontró lo que buscas.",
};

function normalizeFieldErrors(value) {
  if (!value) return {};
  if (Array.isArray(value)) {
    return { general: value.map((item) => String(item)) };
  }
  if (typeof value === "object") {
    return Object.fromEntries(
      Object.entries(value).map(([field, messages]) => [
        field,
        Array.isArray(messages) ? messages.map((item) => String(item)) : [String(messages)],
      ]),
    );
  }
  return { general: [String(value)] };
}

function readFieldErrors(result) {
  const candidates = [
    result?.data?.fieldErrors,
    result?.data?.errors,
    result?.fieldErrors,
    result?.errors,
  ];

  for (const candidate of candidates) {
    if (candidate && typeof candidate === "object") {
      return normalizeFieldErrors(candidate);
    }
  }

  return {};
}

function firstFieldMessage(fieldErrors) {
  for (const messages of Object.values(fieldErrors)) {
    if (Array.isArray(messages) && messages.length > 0) return messages[0];
  }
  return "";
}

function buildErrorMessage(result, status) {
  const fieldErrors = readFieldErrors(result);

  if (status === 400) {
    const fieldMessage = firstFieldMessage(fieldErrors);
    if (fieldMessage) return fieldMessage;
  }

  if (result?.detail) return result.detail;
  if (status === 401) return MESSAGES[401];
  if (status === 403) return MESSAGES[403];
  if (status === 404) return MESSAGES[404];
  if (status >= 500) return "Ocurrió un problema en el servidor. Inténtalo de nuevo en unos momentos.";

  return "No se pudo completar la operación.";
}

export async function apiRequest(path, { method = "GET", body, auth = true, signal } = {}) {
  const headers = { Accept: "application/json" };
  if (body !== undefined) headers["Content-Type"] = "application/json";

  const token = auth ? getToken() : null;
  if (token) headers.Authorization = `Bearer ${token}`;

  let response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
    });
  } catch (error) {
    if (error?.name === "AbortError") throw error;
    throw new ApiError(
      0,
      "Network Error",
      "No se pudo conectar con el servidor. Revisa tu conexión e inténtalo de nuevo.",
    );
  }

  let result = null;
  try {
    result = await response.json();
  } catch {
    result = null;
  }

  if (response.ok && result?.isSuccess !== false) return result;

  if (response.status === 401 && auth && typeof window !== "undefined") {
    window.dispatchEvent(new CustomEvent(UNAUTHORIZED_EVENT));
  }

  const title = result?.title || (response.status >= 500 ? "Server Error" : "Error");
  const message = buildErrorMessage(result, response.status);
  throw new ApiError(response.status, title, message, readFieldErrors(result));
}
