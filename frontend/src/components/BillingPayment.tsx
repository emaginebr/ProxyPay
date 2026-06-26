import { useState, useCallback } from "react";
import { useProxyPay } from "../hooks/useProxyPay";
import { BillingFrequency, PaymentMethod } from "../types/payment";
import type { BillingPaymentProps } from "../types/payment";

export function BillingPayment({
  customer,
  items,
  frequency = BillingFrequency.Monthly,
  paymentMethod = PaymentMethod.CreditCard,
  billingStartDate,
  completionUrl,
  returnUrl,
  onError,
  children,
}: BillingPaymentProps) {
  const { createBilling } = useProxyPay();
  const [loading, setLoading] = useState(false);

  const handleClick = useCallback(async () => {
    if (loading) return;
    setLoading(true);
    try {
      const response = await createBilling(customer, items, {
        frequency,
        paymentMethod,
        billingStartDate,
        completionUrl,
        returnUrl,
      });
      window.location.href = response.url;
    } catch (err) {
      const error = err instanceof Error ? err : new Error(String(err));
      onError?.(error);
      setLoading(false);
    }
  }, [
    loading,
    createBilling,
    customer,
    items,
    frequency,
    paymentMethod,
    billingStartDate,
    completionUrl,
    returnUrl,
    onError,
  ]);

  return (
    <div onClick={handleClick} style={{ display: "inline-block", cursor: loading ? "wait" : "pointer" }}>
      {children}
    </div>
  );
}
