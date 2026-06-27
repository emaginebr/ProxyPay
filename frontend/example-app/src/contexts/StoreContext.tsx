import { createContext, useState, useEffect, useCallback, type ReactNode } from "react";
import { useNAuth } from "nauth-react";
import { storeService } from "../services/storeService";
import type { StoreInfo, StoreInsertInfo, StoreUpdateInfo, StoreCreateResult } from "../types/store";

interface StoreContextType {
  store: StoreInfo | null;
  loading: boolean;
  error: string | null;
  createStore: (data: StoreInsertInfo) => Promise<StoreCreateResult>;
  updateStore: (data: StoreUpdateInfo) => Promise<void>;
  deleteStore: (storeId: number) => Promise<void>;
  refresh: () => Promise<void>;
  clearError: () => void;
}

const StoreContext = createContext<StoreContextType | undefined>(undefined);

export const StoreProvider = ({ children }: { children: ReactNode }) => {
  const { token, isAuthenticated } = useNAuth();
  const [store, setStore] = useState<StoreInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    if (!token) {
      setStore(null);
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const s = await storeService.fetchMyStore(token);
      setStore(s);
      setError(null);
    } catch (err) {
      setStore(null);
      setError(err instanceof Error ? err.message : "Unknown error");
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    if (isAuthenticated && token) {
      refresh();
    } else {
      setStore(null);
      setLoading(false);
    }
  }, [isAuthenticated, token, refresh]);

  const createStore = useCallback(async (data: StoreInsertInfo): Promise<StoreCreateResult> => {
    if (!token) throw new Error("Not authenticated");
    try {
      setError(null);
      const result = await storeService.create(token, data);
      await refresh();
      return result;
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Unknown error";
      setError(errorMsg);
      throw err;
    }
  }, [token, refresh]);

  const updateStore = useCallback(async (data: StoreUpdateInfo): Promise<void> => {
    if (!token) throw new Error("Not authenticated");
    try {
      setError(null);
      await storeService.update(token, data);
      await refresh();
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Unknown error";
      setError(errorMsg);
      throw err;
    }
  }, [token, refresh]);

  const deleteStore = useCallback(async (storeId: number): Promise<void> => {
    if (!token) throw new Error("Not authenticated");
    try {
      setError(null);
      await storeService.delete(token, storeId);
      setStore(null);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Unknown error";
      setError(errorMsg);
      throw err;
    }
  }, [token]);

  const clearError = useCallback(() => { setError(null); }, []);

  const value: StoreContextType = {
    store, loading, error,
    createStore, updateStore, deleteStore, refresh, clearError,
  };

  return <StoreContext.Provider value={value}>{children}</StoreContext.Provider>;
};

export default StoreContext;
