import { useContext } from 'react';
import BillingContext from '../contexts/BillingContext';

/** Custom hook to access the Billing context. Throws if used outside BillingProvider. */
export const useBilling = () => {
  const context = useContext(BillingContext);
  if (!context) throw new Error('useBilling must be used within a BillingProvider');
  return context;
};

export default useBilling;
