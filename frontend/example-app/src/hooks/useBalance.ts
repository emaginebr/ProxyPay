import { useContext } from 'react';
import BalanceContext from '../contexts/BalanceContext';

/** Custom hook to access the Balance context. Throws if used outside BalanceProvider. */
export const useBalance = () => {
  const context = useContext(BalanceContext);
  if (!context) throw new Error('useBalance must be used within a BalanceProvider');
  return context;
};

export default useBalance;
