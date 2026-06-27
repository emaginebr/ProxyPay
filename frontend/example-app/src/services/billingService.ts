import type { BillingRow } from '../types/billing';
import { graphql } from './apiHelpers';

interface CollectionSegment<T> {
  items: T[];
  pageInfo: { hasNextPage: boolean; hasPreviousPage: boolean };
  totalCount: number;
}

/** Billing Service — Manages all API operations related to billings/subscriptions */
class BillingService {
  /** Fetch current user's billings via GraphQL */
  async fetchMyBillings(token: string, skip = 0, take = 20): Promise<BillingRow[]> {
    const data = await graphql<{ myBillings: CollectionSegment<BillingRow> }>(
      token,
      `{ myBillings(skip: ${skip}, take: ${take}) { items { billingId frequency paymentMethod billingStartDate status createdAt customer { customerId name email } } totalCount } }`,
    );
    return data.myBillings?.items ?? [];
  }
}

export const billingService = new BillingService();
export default BillingService;
