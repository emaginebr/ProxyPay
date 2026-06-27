// --- Config ---

export interface ProxyPayConfig {
  baseUrl: string;
  clientId: string;
  tenantId: string;
}

// --- API Objects ---

export interface CustomerInfo {
  name: string;
  documentId: string;
  cellphone: string;
  email: string;
}

export interface InvoiceItem {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discount: number;
}

export type InvoiceStatus =
  | "Pending"
  | "Sent"
  | "Paid"
  | "Overdue"
  | "Cancelled"
  | "Expired";

// --- QR Code (PIX) ---

export interface QRCodeRequest {
  clientId: string;
  customer: CustomerInfo;
  items: InvoiceItem[];
}

export interface QRCodeResponse {
  invoiceId: number;
  invoiceNumber: string;
  brCode: string;
  brCodeBase64: string;
  expiredAt?: string;
}

export interface QRCodeStatusResponse {
  invoiceId: number;
  invoiceNumber: string;
  paid: boolean;
  status: number;
  statusText: InvoiceStatus;
  expiresAt?: string;
}

// --- Invoice ---

export enum PaymentMethod {
  Pix = 1,
  Boleto = 2,
  CreditCard = 3,
  DebitCard = 4,
}

export interface InvoiceRequest {
  clientId: string;
  customer: CustomerInfo;
  paymentMethod: PaymentMethod;
  completionUrl: string;
  returnUrl: string;
  notes?: string;
  discount?: number;
  dueDate?: string;
  items: InvoiceItem[];
}

export interface InvoiceResponse {
  invoiceId: number;
  invoiceNumber: string;
  url: string;
}

// --- Billing ---

export enum BillingFrequency {
  Monthly = 1,
  Quarterly = 2,
  Semiannual = 3,
  Annual = 4,
}

export interface BillingItem {
  billingItemId?: number;
  billingId?: number;
  description: string;
  quantity: number;
  unitPrice: number;
  discount: number;
}

export interface BillingRequest {
  clientId: string;
  frequency: BillingFrequency;
  paymentMethod: PaymentMethod;
  billingStartDate: string;
  completionUrl: string;
  returnUrl: string;
  customer: CustomerInfo;
  items: BillingItem[];
}

export interface BillingResponse {
  billingId: number;
  invoiceId: number;
  invoiceNumber: string;
  url: string;
}

// --- Context ---

export interface ProxyPayContextValue {
  config: ProxyPayConfig;
  createQRCode: (
    customer: CustomerInfo,
    items: InvoiceItem[],
  ) => Promise<QRCodeResponse>;
  checkQRCodeStatus: (invoiceId: number) => Promise<QRCodeStatusResponse>;
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
  ) => Promise<InvoiceResponse>;
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
  ) => Promise<BillingResponse>;
}

// --- Component Props ---

export interface InvoicePaymentProps {
  customer: CustomerInfo;
  items: InvoiceItem[];
  paymentMethod?: PaymentMethod;
  completionUrl: string;
  returnUrl: string;
  notes?: string;
  discount?: number;
  dueDate?: string;
  onError?: (error: Error) => void;
  children?: React.ReactNode;
}

export interface BillingPaymentProps {
  customer: CustomerInfo;
  items: BillingItem[];
  frequency?: BillingFrequency;
  paymentMethod?: PaymentMethod;
  billingStartDate: string;
  completionUrl: string;
  returnUrl: string;
  onError?: (error: Error) => void;
  children?: React.ReactNode;
}

export interface PixPaymentProps {
  customer: CustomerInfo;
  items: InvoiceItem[];
  onSuccess?: (status: QRCodeStatusResponse) => void;
  onError?: (error: Error) => void;
  onStatusChange?: (status: QRCodeStatusResponse) => void;
  pollInterval?: number;
  modalTitle?: string;
  children?: React.ReactNode;
  overlayClassName?: string;
  modalClassName?: string;
}
