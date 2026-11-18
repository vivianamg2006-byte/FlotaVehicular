import { apiRequest } from "./client.js";

export function login(username, password) {
  return apiRequest("/api/auth/login", {
    method: "POST",
    body: { username, password },
    auth: false,
  });
}

export function getCurrentUser() {
  return apiRequest("/api/auth/me");
}

export async function logout() {
  try {
    await apiRequest("/api/auth/logout", { method: "POST" });
  } catch {
    return null;
  }
}
