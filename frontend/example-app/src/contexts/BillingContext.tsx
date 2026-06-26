import { createContext, useState, useCallback, type ReactNode } from "react";
import { useNAuth } from "nauth-react";
import { billingService } from "../services/billingService";
import type { BillingRow } from "../types/billing";

interface BillingContextType {
  billings: BillingRow[];
  loading: boolean;
  error: string | null;
  loadBillings: (skip?: number, take?: number) => Promise<void>;
  refreshBillings: () => Promise<void>;
  clearError: () => void;
}

const BillingContext = createContext<BillingContextType | undefined>(undefined);

export const BillingProvider = ({ children }: { children: ReactNode }) => {
  const { token } = useNAuth();
  const [billings, setBillings] = useState<BillingRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastSkip, setLastSkip] = useState(0);
  const [lastTake, setLastTake] = useState(20);

  const loadBillings = useCallback(async (skip = 0, take = 20): Promise<void> => {
    if (!token) return;
    try {
      setLoading(true);
      setError(null);
      setLastSkip(skip);
      setLastTake(take);
      const result = await billingService.fetchMyBillings(token, skip, take);
      setBillings(result);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Unknown error";
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  }, [token]);

  const refreshBillings = useCallback(async () => {
    await loadBillings(lastSkip, lastTake);
  }, [loadBillings, lastSkip, lastTake]);

  const clearError = useCallback(() => { setError(null); }, []);

  const value: BillingContextType = {
    billings, loading, error,
    loadBillings, refreshBillings, clearError,
  };

  return <BillingContext.Provider value={value}>{children}</BillingContext.Provider>;
};

export default BillingContext;
