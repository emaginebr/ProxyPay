/** Balance Types — Types for the balance and transaction system */

export interface BalanceInfo {
  balance: number;
  totalCredits: number;
  totalDebits: number;
  transactionCount: number;
}

export interface TransactionRow {
  transactionId: number;
  type: number;
  category: number;
  amount: number;
  balance: number;
  description: string;
  createdAt: string;
}
