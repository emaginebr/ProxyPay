import { useContext } from 'react';
import InvoiceContext from '../contexts/InvoiceContext';

/** Custom hook to access the Invoice context. Throws if used outside InvoiceProvider. */
export const useInvoice = () => {
  const context = useContext(InvoiceContext);
  if (!context) throw new Error('useInvoice must be used within a InvoiceProvider');
  return context;
};

export default useInvoice;
