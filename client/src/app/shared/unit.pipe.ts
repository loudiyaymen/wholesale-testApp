import { Pipe, PipeTransform } from '@angular/core';
import { UnitOfMeasure } from '../core/models';

const LABELS: Record<UnitOfMeasure, [one: string, many: string]> = {
  Each: ['unit', 'units'],
  Case: ['case', 'cases'],
  Pallet: ['pallet', 'pallets'],
  Kilogram: ['kg', 'kg'],
  Litre: ['L', 'L'],
};

// {{ a.unit | unit }} -> "case", {{ a.unit | unit: 300 }} -> "cases"
@Pipe({ name: 'unit' })
export class UnitPipe implements PipeTransform {
  transform(unit: UnitOfMeasure | null | undefined, count = 1): string {
    if (!unit) return 'unit';
    const [one, many] = LABELS[unit] ?? ['unit', 'units'];
    return count === 1 ? one : many;
  }
}
