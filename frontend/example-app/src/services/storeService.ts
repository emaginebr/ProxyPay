import type { StoreInfo, StoreInsertInfo, StoreUpdateInfo, StoreCreateResult } from '../types/store';
import { getHeaders, handleResponse, graphql, API_BASE_URL } from './apiHelpers';

/** Store Service — Manages all API operations related to stores */
class StoreService {
  /** Fetch current user's store via GraphQL */
  async fetchMyStore(token: string): Promise<StoreInfo | null> {
    const data = await graphql<{ myStore: StoreInfo | StoreInfo[] }>(
      token,
      `{ myStore { storeId clientId name email billingStrategy createdAt updatedAt } }`,
    );
    const result = data.myStore;
    if (Array.isArray(result)) return result[0] ?? null;
    return result ?? null;
  }

  /** Create a new store */
  async create(token: string, data: StoreInsertInfo): Promise<StoreCreateResult> {
    const res = await fetch(`${API_BASE_URL}/store`, {
      method: "POST",
      headers: getHeaders(token),
      body: JSON.stringify(data),
    });
    return handleResponse(res);
  }

  /** Update an existing store */
  async update(token: string, data: StoreUpdateInfo): Promise<void> {
    const res = await fetch(`${API_BASE_URL}/store`, {
      method: "PUT",
      headers: getHeaders(token),
      body: JSON.stringify(data),
    });
    return handleResponse(res);
  }

  /** Delete a store by ID */
  async delete(token: string, storeId: number): Promise<void> {
    const res = await fetch(`${API_BASE_URL}/store/${storeId}`, {
      method: "DELETE",
      headers: getHeaders(token, false),
    });
    return handleResponse(res);
  }
}

export const storeService = new StoreService();
export default StoreService;
