// These mirror the C# DTOs in WholesaleHub.Application.
// TODO: generate these from the OpenAPI doc instead of keeping them in sync by hand

export type PaymentTerms = 'Net15' | 'Net30' | 'Net60';
export type UnitOfMeasure = 'Each' | 'Case' | 'Pallet' | 'Kilogram' | 'Litre';
export type AgreementStatus = 'Draft' | 'Active' | 'Cancelled' | 'Expired';

export const PAYMENT_TERMS: PaymentTerms[] = ['Net15', 'Net30', 'Net60'];
export const UNITS: UnitOfMeasure[] = ['Each', 'Case', 'Pallet', 'Kilogram', 'Litre'];
export const TERMS = [3, 6, 12, 24, 36];
export const MAX_DISCOUNT = 0.4; // same as PriceAgreement.MaxDiscount on the server

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface Customer {
  id: string;
  legalName: string;
  businessNumber: string;
  contactName: string;
  contactEmail: string;
  contactPhone: string | null;
  shippingAddress: string;
  paymentTerms: PaymentTerms;
  createdAt: string;
}

export type CustomerRequest = Omit<Customer, 'id' | 'createdAt'>;

export interface Product {
  id: string;
  sku: string;
  name: string;
  category: string;
  unit: UnitOfMeasure;
  listPrice: number;
  isActive: boolean;
}

export interface Agreement {
  id: string;
  customerId: string;
  customerName: string;
  productId: string;
  sku: string;
  productName: string;
  unit: UnitOfMeasure;
  unitPrice: number;
  listPriceAtQuote: number;
  annualVolume: number;
  termMonths: number;
  startDate: string;
  endDate: string;
  status: AgreementStatus;
  createdAt: string;
  signedAt: string | null;
  cancelledAt: string | null;
  cancellationReason: string | null;
  annualValue: number;
  discountPercent: number;
}

export interface NewAgreement {
  customerId: string;
  productId: string;
  unitPrice: number;
  termMonths: number;
  startDate: string; // yyyy-MM-dd
  annualVolume: number;
}

export interface Renewal {
  agreementId: string;
  customerId: string;
  customerName: string;
  contactEmail: string;
  productId: string;
  sku: string;
  productName: string;
  unitPrice: number;
  annualVolume: number;
  endDate: string;
  daysRemaining: number;
  annualValue: number;
}

export interface Dashboard {
  customerCount: number;
  activeProductCount: number;
  activeAgreementCount: number;
  draftAgreementCount: number;
  renewalsDueCount: number;
  renewalWindowDays: number;
  annualAgreementValue: number;
}

export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
