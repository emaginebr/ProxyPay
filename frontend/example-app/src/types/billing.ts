/** Billing Types — Types for the billing/subscription system */

export interface BillingRow {
  billingId: number;
  frequency: number;
  paymentMethod: number;
  billingStartDate: string;
  status: number;
  createdAt: string;
  customer?: {
    customerId: number;
    name: string;
    email: string;
  };
}
