import { MAX_DISCOUNT } from '../core/models';

// Client-side copy of the pricing rules so the quote form can react while typing.
// The server is still the source of truth (PriceAgreement.CreateDraft).

export function annualValue(volume: number, unitPrice: number) {
  if (!(volume > 0) || !(unitPrice > 0)) return 0;
  return Math.round(volume * unitPrice * 100) / 100;
}

export function discountPercent(listPrice: number, unitPrice: number) {
  if (!(listPrice > 0) || !(unitPrice > 0)) return 0;
  return Math.round((1 - unitPrice / listPrice) * 1000) / 10;
}

export function priceFloor(listPrice: number) {
  return Math.round(listPrice * (1 - MAX_DISCOUNT) * 100) / 100;
}

export type PriceProblem = 'above-list' | 'below-floor' | null;

export function checkPrice(listPrice: number, unitPrice: number): PriceProblem {
  if (!(listPrice > 0) || !(unitPrice > 0)) return null;
  if (unitPrice > listPrice) return 'above-list';
  if (unitPrice < priceFloor(listPrice)) return 'below-floor';
  return null;
}
