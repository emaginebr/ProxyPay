import { createContext, useMemo } from "react";
import type {
  ProxyPayConfig,
  ProxyPayContextValue,
  CustomerInfo,
  InvoiceItem,
  BillingItem,
  PaymentMethod,
  QRCodeResponse,
  QRCodeStatusResponse,
  InvoiceResponse,
  BillingResponse,
  BillingFrequency,
} from "../types/payment";
import ProxyPayService from "../services/proxyPayService";

const ProxyPayContext = createContext<ProxyPayContextValue | null>(null);

export const ProxyPayProvider = ({
  config,
  children,
}: {
  config: ProxyPayConfig;
  children: React.ReactNode;
}) => {
  const value = useMemo<ProxyPayContextValue>(() => {
    const service = new ProxyPayService();
    service.configure(config);

    return {
      config,
      createQRCode: (customer: CustomerInfo, items: InvoiceItem[]) =>
        service.createQRCode(customer, items),
      checkQRCodeStatus: (invoiceId: number) =>
        service.checkQRCodeStatus(invoiceId),
      createInvoice: (
        customer: CustomerInfo,
        items: InvoiceItem[],
        options: {
          paymentMethod: PaymentMethod;
          completionUrl: string;
          returnUrl: string;
          notes?: string;
          discount?: number;
          dueDate?: string;
        },
      ) => service.createInvoice(customer, items, options),
      createBilling: (
        customer: CustomerInfo,
        items: BillingItem[],
        options: {
          frequency: BillingFrequency;
          paymentMethod: PaymentMethod;
          billingStartDate: string;
          completionUrl: string;
          returnUrl: string;
        },
      ) => service.createBilling(customer, items, options),
    };
  }, [config]);

  return (
    <ProxyPayContext.Provider value={value}>{children}</ProxyPayContext.Provider>
  );
};

export default ProxyPayContext;
