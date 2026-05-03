import { UnitPipe } from './unit.pipe';

describe('UnitPipe', () => {
  const pipe = new UnitPipe();

  it('pluralizes countable units', () => {
    expect(pipe.transform('Case')).toBe('case');
    expect(pipe.transform('Case', 300)).toBe('cases');
    expect(pipe.transform('Each', 2)).toBe('units');
  });

  it('uses short forms for weight and volume', () => {
    expect(pipe.transform('Kilogram', 12000)).toBe('kg');
    expect(pipe.transform('Litre')).toBe('L');
  });
});
