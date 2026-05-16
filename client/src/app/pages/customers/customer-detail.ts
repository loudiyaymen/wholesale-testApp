import { CurrencyPipe, DatePipe } from '@angular/common';
import { UnitPipe } from '../../shared/unit.pipe';
import { ChangeDetectionStrategy, Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmTableImports } from '@spartan-ng/helm/table';
import { AgreementsApi, CustomersApi } from '../../core/api';
import { Agreement, Customer } from '../../core/models';
import { StatusBadge } from '../../shared/status-badge';

@Component({
  selector: 'app-customer-detail',
  imports: [RouterLink, CurrencyPipe, DatePipe, HlmButtonImports, HlmTableImports, StatusBadge, UnitPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (customer(); as c) {
      <div class="mb-8 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 class="text-4xl font-bold">{{ c.legalName }}</h1>
          <dl class="mt-3 grid grid-cols-[auto_1fr] gap-x-6 gap-y-1 text-[15px]">
            <dt class="text-muted-foreground">Business number</dt><dd class="tabular-nums">{{ c.businessNumber }}</dd>
            <dt class="text-muted-foreground">Contact</dt>
            <dd>{{ c.contactName }}, <a [href]="'mailto:' + c.contactEmail" class="text-primary underline">{{ c.contactEmail }}</a>
              @if (c.contactPhone) { , {{ c.contactPhone }} }</dd>
            <dt class="text-muted-foreground">Ships to</dt><dd>{{ c.shippingAddress }}</dd>
            <dt class="text-muted-foreground">Payment terms</dt><dd>{{ c.paymentTerms }}</dd>
          </dl>
        </div>
        <div class="flex gap-2">
          <a hlmBtn variant="outline" [routerLink]="['/customers', c.id, 'edit']">Edit</a>
          <a hlmBtn [routerLink]="['/agreements/new']" [queryParams]="{ customerId: c.id }">Quote a price</a>
        </div>
      </div>

      <h2 class="mb-3 text-2xl font-semibold">Price agreements</h2>
      @if (agreements().length) {
        <div hlmTableContainer class="border-border border bg-card">
          <table hlmTable>
            <thead hlmTableHeader>
              <tr hlmTableRow>
                <th hlmTableHead>Product</th>
                <th hlmTableHead class="text-right">Price</th>
                <th hlmTableHead class="text-right">Off list</th>
                <th hlmTableHead>Runs</th>
                <th hlmTableHead class="text-right">Per year</th>
                <th hlmTableHead>Status</th>
              </tr>
            </thead>
            <tbody hlmTableBody>
              @for (a of agreements(); track a.id) {
                <tr hlmTableRow>
                  <td hlmTableCell><a [routerLink]="['/agreements', a.id]" class="font-medium hover:underline">{{ a.productName }}</a></td>
                  <td hlmTableCell class="text-right tabular-nums">{{ a.unitPrice | currency: 'CAD' : 'symbol-narrow' }}/{{ a.unit | unit }}</td>
                  <td hlmTableCell class="text-right tabular-nums">{{ a.discountPercent }}%</td>
                  <td hlmTableCell>{{ a.startDate | date: 'MMM d, y' }} to {{ a.endDate | date: 'MMM d, y' }}</td>
                  <td hlmTableCell class="text-right tabular-nums">{{ a.annualValue | currency: 'CAD' : 'symbol-narrow' : '1.0-0' }}</td>
                  <td hlmTableCell><app-status-badge [status]="a.status" /></td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <p class="text-muted-foreground">No agreements with this customer yet.</p>
      }
    } @else {
      <p class="text-muted-foreground">Loading…</p>
    }
  `,
})
export class CustomerDetail implements OnInit {
  id = input.required<string>();

  private customers = inject(CustomersApi);
  private agreementsApi = inject(AgreementsApi);

  protected customer = signal<Customer | null>(null);
  protected agreements = signal<Agreement[]>([]);

  ngOnInit() {
    this.customers.get(this.id()).subscribe((c) => this.customer.set(c));
    // TODO: paging. 100 should be plenty per customer for now
    this.agreementsApi.list({ customerId: this.id(), pageSize: 100 }).subscribe((r) => this.agreements.set(r.items));
  }
}
