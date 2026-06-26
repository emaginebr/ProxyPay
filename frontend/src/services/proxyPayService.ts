import type {
  ProxyPayConfig,
  CustomerInfo,
  InvoiceItem,
  BillingItem,
  PaymentMethod,
  QRCodeResponse,
  QRCodeStatusResponse,
  InvoiceResponse,
  BillingResponse,
  BillingFrequency,
} from '../types/payment';

interface ProxyPayServiceConfig {
  onError?: (error: Error) => void;
}

/** ProxyPay Service — Manages all API operations related to payments */
class ProxyPayService {
  private serviceConfig: ProxyPayServiceConfig;
  private config: ProxyPayConfig | null = null;

  constructor(serviceConfig: ProxyPayServiceConfig = {}) {
    this.serviceConfig = serviceConfig;
  }

  /** Configure the service with ProxyPay credentials */
  configure(config: ProxyPayConfig): void {
    this.config = config;
  }

  private getConfig(): ProxyPayConfig {
    if (!this.config) {
      throw new Error('ProxyPayService not configured. Call configure() first.');
    }
    return this.config;
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const body = await response.text();
      const error = new Error(body || `ProxyPay API error: ${response.status}`);
      this.serviceConfig.onError?.(error);
      throw error;
    }
    return response.json();
  }

  private getHeaders(): Record<string, string> {
    const config = this.getConfig();
    return {
      'Content-Type': 'application/json',
      'X-Tenant-Id': config.tenantId,
    };
  }

  /** Create a PIX QR Code for payment */
  async createQRCode(customer: CustomerInfo, items: InvoiceItem[]): Promise<QRCodeResponse> {
    const config = this.getConfig();
    const response = await fetch(`${config.baseUrl}/payment/qrcode`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify({ clientId: config.clientId, customer, items }),
    });
    return this.handleResponse<QRCodeResponse>(response);
  }

  /** Check the status of a PIX QR Code payment */
  async checkQRCodeStatus(invoiceId: number): Promise<QRCodeStatusResponse> {
    const config = this.getConfig();
    const response = await fetch(`${config.baseUrl}/payment/qrcode/status/${invoiceId}`, {
      headers: this.getHeaders(),
    });
    return this.handleResponse<QRCodeStatusResponse>(response);
  }

  /** Create an invoice */
  async createInvoice(
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
  ): Promise<InvoiceResponse> {
    const config = this.getConfig();
    const response = await fetch(`${config.baseUrl}/payment/invoice`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify({ clientId: config.clientId, customer, items, ...options }),
    });
    return this.handleResponse<InvoiceResponse>(response);
  }

  /** Create a billing/subscription */
  async createBilling(
    customer: CustomerInfo,
    items: BillingItem[],
    options: {
      frequency: BillingFrequency;
      paymentMethod: PaymentMethod;
      billingStartDate: string;
      completionUrl: string;
      returnUrl: string;
    },
  ): Promise<BillingResponse> {
    const config = this.getConfig();
    const response = await fetch(`${config.baseUrl}/payment/billing`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify({
        clientId: config.clientId,
        customer,
        items: items.map((item) => ({
          billingItemId: item.billingItemId ?? 0,
          billingId: item.billingId ?? 0,
          ...item,
        })),
        ...options,
      }),
    });
    return this.handleResponse<BillingResponse>(response);
  }
}

export const proxyPayService = new ProxyPayService();
export default ProxyPayService;
