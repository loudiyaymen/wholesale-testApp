import { annualValue, checkPrice, discountPercent, priceFloor } from './pricing';

describe('pricing', () => {
  it('annual value is volume x price, rounded to cents', () => {
    expect(annualValue(1234, 24.99)).toBe(30837.66);
    expect(annualValue(0, 10)).toBe(0);
    expect(annualValue(10, NaN)).toBe(0);
  });

  it('discount percent to one decimal', () => {
    expect(discountPercent(50, 42.5)).toBe(15);
    expect(discountPercent(3, 2)).toBe(33.3);
  });

  it('floor is 40% off list', () => {
    expect(priceFloor(1.8)).toBe(1.08);
  });

  it('flags prices above list or below the floor', () => {
    expect(checkPrice(100, 100.01)).toBe('above-list');
    expect(checkPrice(100, 59.99)).toBe('below-floor');
    expect(checkPrice(100, 60)).toBeNull();
    expect(checkPrice(100, 100)).toBeNull();
    expect(checkPrice(0, 5)).toBeNull(); // no product picked yet
  });
});
