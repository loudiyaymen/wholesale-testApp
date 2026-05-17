import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { UnitPipe } from '../../shared/unit.pipe';
import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { HlmTableImports } from '@spartan-ng/helm/table';
import { HlmTabsImports } from '@spartan-ng/helm/tabs';
import { catchError, of, switchMap } from 'rxjs';
import { AgreementsApi } from '../../core/api';
import { Agreement, AgreementStatus, Paged } from '../../core/models';
import { Pager } from '../../shared/pager';
import { StatusBadge } from '../../shared/status-badge';

type Filter = AgreementStatus | 'All';

@Component({
  selector: 'app-agreement-list',
  imports: [RouterLink, CurrencyPipe, DatePipe, HlmTableImports, HlmTabsImports, StatusBadge, Pager, DecimalPipe, UnitPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1 class="mb-6 text-4xl font-bold">Price agreements</h1>

    <hlm-tabs [tab]="filter()" (tabActivated)="setFilter($any($event))" class="mb-4">
      <hlm-tabs-list aria-label="Filter by status">
        @for (f of filters; track f.value) {
          <button [hlmTabsTrigger]="f.value">{{ f.label }}</button>
        }
      </hlm-tabs-list>
    </hlm-tabs>

    @if (result(); as r) {
      @if (r.items.length) {
        <div hlmTableContainer class="border-border border bg-card">
          <table hlmTable>
            <thead hlmTableHeader>
              <tr hlmTableRow>
                <th hlmTableHead>Customer</th>
                <th hlmTableHead>Product</th>
                <th hlmTableHead class="text-right">Price</th>
                <th hlmTableHead class="text-right">Volume / yr</th>
                <th hlmTableHead>Ends</th>
                <th hlmTableHead class="text-right">Per year</th>
                <th hlmTableHead>Status</th>
              </tr>
            </thead>
            <tbody hlmTableBody>
              @for (a of r.items; track a.id) {
                <tr hlmTableRow>
                  <td hlmTableCell><a [routerLink]="['/agreements', a.id]" class="font-medium hover:underline">{{ a.customerName }}</a></td>
                  <td hlmTableCell>{{ a.productName }}</td>
                  <td hlmTableCell class="text-right tabular-nums">{{ a.unitPrice | currency: 'CAD' : 'symbol-narrow' }}</td>
                  <td hlmTableCell class="text-right tabular-nums">{{ a.annualVolume | number }} {{ a.unit | unit: a.annualVolume }}</td>
                  <td hlmTableCell>{{ a.endDate | date: 'MMM d, y' }}</td>
                  <td hlmTableCell class="text-right tabular-nums">{{ a.annualValue | currency: 'CAD' : 'symbol-narrow' : '1.0-0' }}</td>
                  <td hlmTableCell><app-status-badge [status]="a.status" /></td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <app-pager [page]="r.page" [totalPages]="r.totalPages" (pageChange)="page.set($event)" />
      } @else {
        <p class="text-muted-foreground">No agreements here.</p>
      }
    } @else {
      <p class="text-muted-foreground">Loading…</p>
    }
  `,
})
export class AgreementList {
  // ?status=Draft from the "See quotes" link on the Today page
  status = input<AgreementStatus>();

  private api = inject(AgreementsApi);

  protected readonly filters: { value: Filter; label: string }[] = [
    { value: 'All', label: 'All' },
    { value: 'Draft', label: 'Quotes' },
    { value: 'Active', label: 'Signed' },
    { value: 'Expired', label: 'Expired' },
    { value: 'Cancelled', label: 'Cancelled' },
  ];

  protected filter = linkedSignal<Filter>(() => this.status() ?? 'All');
  protected page = signal(1);
  protected result = signal<Paged<Agreement> | null>(null);

  // re-query whenever filter or page changes
  private query = computed(() => ({ filter: this.filter(), page: this.page() }));

  constructor() {
    toObservable(this.query)
      .pipe(
        switchMap(({ filter, page }) =>
          this.api.list({ status: filter === 'All' ? null : filter, page }).pipe(catchError(() => of(null))),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((r) => this.result.set(r));
  }

  setFilter(f: Filter) {
    this.filter.set(f);
    this.page.set(1); // new filter = back to page 1
    this.result.set(null);
  }
}
