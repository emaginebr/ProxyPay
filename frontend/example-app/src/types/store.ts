/** Store Types — Types for the store management system */

export interface StoreInfo {
  storeId: number;
  clientId: string;
  name: string;
  email: string;
  billingStrategy: number;
  createdAt: string;
  updatedAt: string;
}

export interface StoreInsertInfo {
  name: string;
  email: string;
  billingStrategy: number;
}

export interface StoreUpdateInfo {
  storeId: number;
  name: string;
  email: string;
  billingStrategy: number;
}

export interface StoreCreateResult {
  storeId: number;
  clientId: string;
}
