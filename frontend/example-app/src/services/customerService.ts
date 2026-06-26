import type { CustomerRow } from '../types/customer';
import { graphql } from './apiHelpers';

interface CollectionSegment<T> {
  items: T[];
  pageInfo: { hasNextPage: boolean; hasPreviousPage: boolean };
  totalCount: number;
}

/** Customer Service — Manages all API operations related to customers */
class CustomerService {
  /** Fetch current user's customers via GraphQL */
  async fetchMyCustomers(token: string, skip = 0, take = 20): Promise<CustomerRow[]> {
    const data = await graphql<{ myCustomers: CollectionSegment<CustomerRow> }>(
      token,
      `{ myCustomers(skip: ${skip}, take: ${take}) { items { customerId name email documentId cellphone createdAt } totalCount } }`,
    );
    return data.myCustomers?.items ?? [];
  }
}

export const customerService = new CustomerService();
export default CustomerService;
