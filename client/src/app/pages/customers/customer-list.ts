import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmTableImports } from '@spartan-ng/helm/table';
import { Subject, catchError, debounceTime, distinctUntilChanged, map, merge, of, startWith, switchMap } from 'rxjs';
import { CustomersApi } from '../../core/api';
import { Customer, Paged } from '../../core/models';
import { Pager } from '../../shared/pager';

@Component({
  selector: 'app-customer-list',
  imports: [ReactiveFormsModule, RouterLink, HlmButtonImports, HlmInputImports, HlmTableImports, Pager],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mb-6 flex flex-wrap items-end justify-between gap-4">
      <h1 class="text-4xl font-bold">Customers</h1>
      <a hlmBtn routerLink="/customers/new">Add customer</a>
    </div>

    <input hlmInput type="search" class="mb-4 w-full max-w-md bg-card" [formControl]="search"
           placeholder="Search by name, business number or contact" aria-label="Search customers" />

    @if (result(); as r) {
      @if (r.items.length) {
        <div hlmTableContainer class="border-border border bg-card">
          <table hlmTable>
            <thead hlmTableHeader>
              <tr hlmTableRow>
                <th hlmTableHead>Customer</th>
                <th hlmTableHead>Business number</th>
                <th hlmTableHead>Contact</th>
                <th hlmTableHead>Terms</th>
              </tr>
            </thead>
            <tbody hlmTableBody>
              @for (c of r.items; track c.id) {
                <tr hlmTableRow>
                  <td hlmTableCell class="font-medium"><a [routerLink]="['/customers', c.id]" class="hover:underline">{{ c.legalName }}</a></td>
                  <td hlmTableCell class="tabular-nums">{{ c.businessNumber }}</td>
                  <td hlmTableCell>{{ c.contactName }} <span class="text-muted-foreground">({{ c.contactEmail }})</span></td>
                  <td hlmTableCell>{{ c.paymentTerms }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <app-pager [page]="r.page" [totalPages]="r.totalPages" (pageChange)="page$.next($event)" />
      } @else if (search.value) {
        <p class="text-muted-foreground">No customers match "{{ search.value }}".</p>
      } @else {
        <p class="text-muted-foreground">No customers yet. <a routerLink="/customers/new" class="text-primary underline">Add the first one</a>.</p>
      }
    } @else {
      <p class="text-muted-foreground">Loading customers…</p>
    }
  `,
})
export class CustomerList {
  private api = inject(CustomersApi);

  protected search = new FormControl('', { nonNullable: true });
  protected page$ = new Subject<number>();
  protected result = signal<Paged<Customer> | null>(null);

  constructor() {
    const typed = this.search.valueChanges.pipe(debounceTime(300), distinctUntilChanged(), map(() => 1));

    // switchMap cancels the old request if the user keeps typing, so results can't arrive out of order
    merge(typed, this.page$)
      .pipe(
        startWith(1),
        switchMap((page) => this.api.search(this.search.value, page).pipe(catchError(() => of(null)))),
        takeUntilDestroyed(),
      )
      .subscribe((r) => this.result.set(r));
  }
}
