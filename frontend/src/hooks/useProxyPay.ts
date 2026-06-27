import { useContext } from 'react';
import ProxyPayContext from '../contexts/ProxyPayContext';
import type { ProxyPayContextValue } from '../types/payment';

/** Custom hook to access the ProxyPay context. Throws if used outside ProxyPayProvider. */
export const useProxyPay = (): ProxyPayContextValue => {
  const context = useContext(ProxyPayContext);
  if (!context) {
    throw new Error('useProxyPay must be used within a <ProxyPayProvider>');
  }
  return context;
};

export default useProxyPay;
