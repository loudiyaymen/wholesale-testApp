import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { Renewal } from '../core/models';
import { addDays } from './forms';

// The renewals list, styled like the board at a loading dock:
// big day count on the left, who/what in the middle, value + action on the right.
// Anything inside `urgentDays` gets the yellow bar.
@Component({
  selector: 'app-dock-board',
  imports: [RouterLink, CurrencyPipe, DatePipe, HlmButtonImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ol class="divide-y divide-border border-y border-border bg-card">
      @for (r of renewals(); track r.agreementId) {
        <li class="relative grid grid-cols-[5.5rem_1fr] items-center gap-x-5 gap-y-3 py-4 pr-4 pl-5 sm:grid-cols-[6.5rem_1fr_auto]">
          @if (r.daysRemaining <= urgentDays()) {
            <span class="absolute inset-y-0 left-0 w-1.5 bg-signal" aria-hidden="true"></span>
          }
          <p class="font-display text-right leading-none">
            <span class="block text-5xl font-semibold tabular-nums" data-testid="days">{{ r.daysRemaining }}</span>
            <span class="text-muted-foreground text-sm">{{ r.daysRemaining === 1 ? 'day left' : 'days left' }}</span>
          </p>

          <div class="min-w-0">
            <a [routerLink]="['/customers', r.customerId]" class="block truncate text-lg font-semibold hover:underline">{{ r.customerName }}</a>
            <p class="text-muted-foreground truncate">
              <a [routerLink]="['/agreements', r.agreementId]" class="hover:underline">{{ r.productName }}</a>
              at {{ r.unitPrice | currency: 'CAD' : 'symbol-narrow' }}, ends {{ r.endDate | date: 'MMM d' }}
            </p>
          </div>

          <div class="col-span-2 flex items-center justify-between gap-4 sm:col-span-1 sm:justify-end">
            <span class="tabular-nums">{{ r.annualValue | currency: 'CAD' : 'symbol-narrow' : '1.0-0' }}/yr</span>
            <a
              hlmBtn
              variant="outline"
              size="sm"
              [routerLink]="['/agreements/new']"
              [queryParams]="{ customerId: r.customerId, productId: r.productId, startDate: dayAfter(r.endDate), unitPrice: r.unitPrice, annualVolume: r.annualVolume }"
              >Quote renewal</a
            >
          </div>
        </li>
      }
    </ol>
  `,
})
export class DockBoard {
  renewals = input.required<Renewal[]>();
  urgentDays = input(14);

  protected dayAfter = (iso: string) => addDays(iso, 1);
}
