import { createContext, useState, useCallback, type ReactNode } from "react";
import { useNAuth } from "nauth-react";
import { customerService } from "../services/customerService";
import type { CustomerRow } from "../types/customer";

interface CustomerContextType {
  customers: CustomerRow[];
  loading: boolean;
  error: string | null;
  loadCustomers: (skip?: number, take?: number) => Promise<void>;
  refreshCustomers: () => Promise<void>;
  clearError: () => void;
}

const CustomerContext = createContext<CustomerContextType | undefined>(undefined);

export const CustomerProvider = ({ children }: { children: ReactNode }) => {
  const { token } = useNAuth();
  const [customers, setCustomers] = useState<CustomerRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastSkip, setLastSkip] = useState(0);
  const [lastTake, setLastTake] = useState(20);

  const loadCustomers = useCallback(async (skip = 0, take = 20): Promise<void> => {
    if (!token) return;
    try {
      setLoading(true);
      setError(null);
      setLastSkip(skip);
      setLastTake(take);
      const result = await customerService.fetchMyCustomers(token, skip, take);
      setCustomers(result);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Unknown error";
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  }, [token]);

  const refreshCustomers = useCallback(async () => {
    await loadCustomers(lastSkip, lastTake);
  }, [loadCustomers, lastSkip, lastTake]);

  const clearError = useCallback(() => { setError(null); }, []);

  const value: CustomerContextType = {
    customers, loading, error,
    loadCustomers, refreshCustomers, clearError,
  };

  return <CustomerContext.Provider value={value}>{children}</CustomerContext.Provider>;
};

export default CustomerContext;
