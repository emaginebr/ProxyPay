import type { InvoiceRow } from '../types/invoice';
import { graphql } from './apiHelpers';

interface CollectionSegment<T> {
  items: T[];
  pageInfo: { hasNextPage: boolean; hasPreviousPage: boolean };
  totalCount: number;
}

/** Invoice Service — Manages all API operations related to invoices */
class InvoiceService {
  /** Fetch current user's invoices via GraphQL */
  async fetchMyInvoices(token: string, skip = 0, take = 20): Promise<InvoiceRow[]> {
    const data = await graphql<{ myInvoices: CollectionSegment<InvoiceRow> }>(
      token,
      `{ myInvoices(skip: ${skip}, take: ${take}) { items { invoiceId invoiceNumber status paymentMethod discount dueDate paidAt createdAt customer { name email } } totalCount } }`,
    );
    return data.myInvoices?.items ?? [];
  }
}

export const invoiceService = new InvoiceService();
export default InvoiceService;
