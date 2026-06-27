import { useContext } from 'react';
import CustomerContext from '../contexts/CustomerContext';

/** Custom hook to access the Customer context. Throws if used outside CustomerProvider. */
export const useCustomer = () => {
  const context = useContext(CustomerContext);
  if (!context) throw new Error('useCustomer must be used within a CustomerProvider');
  return context;
};

export default useCustomer;
