const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
const TENANT_ID = import.meta.env.VITE_TENANT_ID;

/** Get request headers with authentication token */
export function getHeaders(token: string, json = true): Record<string, string> {
  const h: Record<string, string> = {
    Authorization: `Bearer ${token}`,
    "X-Tenant-Id": TENANT_ID,
  };
  if (json) h["Content-Type"] = "application/json";
  return h;
}

/** Handle API response, throwing on error */
export async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `API error: ${response.status}`);
  }
  if (response.status === 204) return undefined as T;
  return response.json();
}

/** Execute a GraphQL query */
export async function graphql<T>(token: string, query: string): Promise<T> {
  const res = await fetch(`${API_BASE_URL}/graphql`, {
    method: "POST",
    headers: getHeaders(token),
    body: JSON.stringify({ query }),
  });
  const json = await handleResponse<{ data: T; errors?: { message: string }[] }>(res);
  if (json.errors?.length) {
    throw new Error(json.errors[0].message);
  }
  return json.data;
}

export { API_BASE_URL, TENANT_ID };
