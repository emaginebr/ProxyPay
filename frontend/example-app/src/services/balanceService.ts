import type { BalanceInfo } from '../types/balance';
import type { TransactionRow } from '../types/balance';
import { graphql } from './apiHelpers';

interface CollectionSegment<T> {
  items: T[];
  pageInfo: { hasNextPage: boolean; hasPreviousPage: boolean };
  totalCount: number;
}

/** Balance Service — Manages all API operations related to balance and transactions */
class BalanceService {
  /** Fetch current user's balance via GraphQL */
  async fetchMyBalance(token: string): Promise<BalanceInfo> {
    const data = await graphql<{ myBalance: BalanceInfo | BalanceInfo[] }>(
      token,
      `{ myBalance { balance totalCredits totalDebits transactionCount } }`,
    );
    const result = data.myBalance;
    const fallback = { balance: 0, totalCredits: 0, totalDebits: 0, transactionCount: 0 };
    if (Array.isArray(result)) return result[0] ?? fallback;
    return result ?? fallback;
  }

  /** Fetch current user's transactions via GraphQL */
  async fetchMyTransactions(token: string, skip = 0, take = 20): Promise<TransactionRow[]> {
    const data = await graphql<{ myTransactions: CollectionSegment<TransactionRow> }>(
      token,
      `{ myTransactions(skip: ${skip}, take: ${take}) { items { transactionId type category amount balance description createdAt } totalCount } }`,
    );
    return data.myTransactions?.items ?? [];
  }
}

export const balanceService = new BalanceService();
export default BalanceService;
