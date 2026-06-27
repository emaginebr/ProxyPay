import { createContext, useState, useCallback, type ReactNode } from "react";
import { useNAuth } from "nauth-react";
import { balanceService } from "../services/balanceService";
import type { BalanceInfo, TransactionRow } from "../types/balance";

interface BalanceContextType {
  balance: BalanceInfo | null;
  transactions: TransactionRow[];
  loading: boolean;
  error: string | null;
  loadBalance: () => Promise<void>;
  loadTransactions: (skip?: number, take?: number) => Promise<void>;
  refreshBalance: () => Promise<void>;
  clearError: () => void;
}

const BalanceContext = createContext<BalanceContextType | undefined>(undefined);

export const BalanceProvider = ({ children }: { children: ReactNode }) => {
  const { token } = useNAuth();
  const [balance, setBalance] = useState<BalanceInfo | null>(null);
  const [transactions, setTransactions] = useState<TransactionRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadBalance = useCallback(async (): Promise<void> => {
    if (!token) return;
    try {
      setLoading(true);
      setError(null);
      const result = await balanceService.fetchMyBalance(token);
      setBalance(result);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Unknown error";
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  }, [token]);

  const loadTransactions = useCallback(async (skip = 0, take = 20): Promise<void> => {
    if (!token) return;
    try {
      setLoading(true);
      setError(null);
      const result = await balanceService.fetchMyTransactions(token, skip, take);
      setTransactions(result);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Unknown error";
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  }, [token]);

  const refreshBalance = useCallback(async () => {
    await loadBalance();
  }, [loadBalance]);

  const clearError = useCallback(() => { setError(null); }, []);

  const value: BalanceContextType = {
    balance, transactions, loading, error,
    loadBalance, loadTransactions, refreshBalance, clearError,
  };

  return <BalanceContext.Provider value={value}>{children}</BalanceContext.Provider>;
};

export default BalanceContext;
