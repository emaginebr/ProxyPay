import { createContext, useState, useCallback, type ReactNode } from "react";
import { useNAuth } from "nauth-react";
import { invoiceService } from "../services/invoiceService";
import type { InvoiceRow } from "../types/invoice";

interface InvoiceContextType {
  invoices: InvoiceRow[];
  loading: boolean;
  error: string | null;
  loadInvoices: (skip?: number, take?: number) => Promise<void>;
  refreshInvoices: () => Promise<void>;
  clearError: () => void;
}

const InvoiceContext = createContext<InvoiceContextType | undefined>(undefined);

export const InvoiceProvider = ({ children }: { children: ReactNode }) => {
  const { token } = useNAuth();
  const [invoices, setInvoices] = useState<InvoiceRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastSkip, setLastSkip] = useState(0);
  const [lastTake, setLastTake] = useState(20);

  const loadInvoices = useCallback(async (skip = 0, take = 20): Promise<void> => {
    if (!token) return;
    try {
      setLoading(true);
      setError(null);
      setLastSkip(skip);
      setLastTake(take);
      const result = await invoiceService.fetchMyInvoices(token, skip, take);
      setInvoices(result);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Unknown error";
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  }, [token]);

  const refreshInvoices = useCallback(async () => {
    await loadInvoices(lastSkip, lastTake);
  }, [loadInvoices, lastSkip, lastTake]);

  const clearError = useCallback(() => { setError(null); }, []);

  const value: InvoiceContextType = {
    invoices, loading, error,
    loadInvoices, refreshInvoices, clearError,
  };

  return <InvoiceContext.Provider value={value}>{children}</InvoiceContext.Provider>;
};

export default InvoiceContext;
