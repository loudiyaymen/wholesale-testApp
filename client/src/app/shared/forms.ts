import { AbstractControl, FormGroup, ValidatorFn } from '@angular/forms';
import { ApiError } from '../core/api-error';

// puts the server's field errors on the matching controls as { server: '...' }
export function applyServerErrors(form: FormGroup, error: unknown): boolean {
  if (!(error instanceof ApiError) || !error.isValidation) return false;

  for (const [field, messages] of Object.entries(error.fieldErrors)) {
    // edge case: server sends a field the form doesn't have (e.g. "customerId" on a hidden value). just ignore it
    form.get(field)?.setErrors({ server: messages.join(' ') });
  }
  form.markAllAsTouched();
  return true;
}

export function serverError(control: AbstractControl | null): string | null {
  return (control?.errors?.['server'] as string | undefined) ?? null;
}

// yyyy-MM-dd in UTC because that's what the API compares against
export function todayIso(now = new Date()) {
  return now.toISOString().slice(0, 10);
}

export function addDays(iso: string, days: number) {
  const d = new Date(`${iso}T00:00:00Z`);
  d.setUTCDate(d.getUTCDate() + days);
  return d.toISOString().slice(0, 10);
}

export const notInPast =
  (today: () => string = todayIso): ValidatorFn =>
  (c) =>
    c.value && c.value < today() ? { pastDate: true } : null;
