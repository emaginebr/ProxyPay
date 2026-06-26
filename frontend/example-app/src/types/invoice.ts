/** Invoice Types — Types for the invoice system */

export interface InvoiceRow {
  invoiceId: number;
  invoiceNumber: string;
  status: number;
  paymentMethod: number;
  discount: number;
  dueDate: string;
  paidAt: string | null;
  createdAt: string;
  customer?: {
    name: string;
    email: string;
  };
}
