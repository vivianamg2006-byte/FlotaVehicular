const SESSION_KEY = "flota.session";
const FLASH_KEY = "flota.flash";

let memorySession = null;
let pendingFlash = "";

function readStored(key) {
  try {
    const raw = window.sessionStorage.getItem(key);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function writeStored(key, value) {
  try {
    window.sessionStorage.setItem(key, JSON.stringify(value));
  } catch {
    return;
  }
}

function removeStored(key) {
  try {
    window.sessionStorage.removeItem(key);
  } catch {
    return;
  }
}

function isExpired(expiresAt) {
  const time = new Date(expiresAt).getTime();
  return !Number.isFinite(time) || time <= Date.now();
}

function isValidSession(value) {
  return Boolean(value?.token && value?.expiresAt && !isExpired(value.expiresAt));
}

export function loadSession() {
  const candidate = memorySession ?? readStored(SESSION_KEY);

  if (!isValidSession(candidate)) {
    clearSession();
    return null;
  }

  memorySession = candidate;
  writeStored(SESSION_KEY, candidate);
  return candidate;
}

export function saveSession(session) {
  memorySession = session;
  writeStored(SESSION_KEY, session);
}

export function clearSession() {
  memorySession = null;
  removeStored(SESSION_KEY);
}

export function getToken() {
  return loadSession()?.token ?? null;
}

export function setFlash(message) {
  pendingFlash = message;
  writeStored(FLASH_KEY, { message });
}

export function consumeFlash() {
  const stored = readStored(FLASH_KEY);
  const message = stored?.message || pendingFlash;
  pendingFlash = message;
  removeStored(FLASH_KEY);
  return message;
}

export function clearFlash() {
  pendingFlash = "";
  removeStored(FLASH_KEY);
}
