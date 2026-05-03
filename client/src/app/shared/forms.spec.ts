import { FormControl, FormGroup } from '@angular/forms';
import { ApiError } from '../core/api-error';
import { addDays, applyServerErrors, notInPast, serverError, todayIso } from './forms';

describe('form helpers', () => {
  it('puts server field errors on the matching controls', () => {
    const form = new FormGroup({ sku: new FormControl('x'), name: new FormControl('y') });

    const handled = applyServerErrors(form, new ApiError(400, 'bad', { sku: ['SKU already exists.'], somethingElse: ['ignored'] }));

    expect(handled).toBe(true);
    expect(serverError(form.controls.sku)).toBe('SKU already exists.');
    expect(form.controls.name.valid).toBe(true);
  });

  it('ignores errors that are not validation errors', () => {
    const form = new FormGroup({ sku: new FormControl('x') });
    expect(applyServerErrors(form, new ApiError(409, 'conflict'))).toBe(false);
    expect(applyServerErrors(form, new Error('boom'))).toBe(false);
  });

  it('notInPast', () => {
    const v = notInPast(() => '2026-06-15');
    expect(v(new FormControl('2026-06-14'))).toEqual({ pastDate: true });
    expect(v(new FormControl('2026-06-15'))).toBeNull();
    expect(v(new FormControl(''))).toBeNull();
  });

  it('date helpers work in UTC', () => {
    expect(todayIso(new Date('2026-03-01T23:30:00Z'))).toBe('2026-03-01');
    expect(addDays('2026-02-28', 1)).toBe('2026-03-01');
    expect(addDays('2026-12-31', 1)).toBe('2027-01-01');
  });
});
