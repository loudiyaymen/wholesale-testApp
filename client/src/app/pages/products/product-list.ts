import { CurrencyPipe } from '@angular/common';
import { UnitPipe } from '../../shared/unit.pipe';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmTableImports } from '@spartan-ng/helm/table';
import { HlmTabsImports } from '@spartan-ng/helm/tabs';
import { Subject, catchError, combineLatest, debounceTime, of, startWith, switchMap } from 'rxjs';
import { ProductsApi } from '../../core/api';
import { Paged, Product } from '../../core/models';
import { Pager } from '../../shared/pager';

@Component({
  selector: 'app-product-list',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, HlmButtonImports, HlmInputImports, HlmTableImports, HlmTabsImports, Pager, UnitPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './product-list.html',
})
export class ProductList {
  private api = inject(ProductsApi);

  protected search = new FormControl('', { nonNullable: true });
  protected show = signal<'selling' | 'all'>('selling');
  protected page$ = new Subject<number>();
  private reload$ = new Subject<void>();
  protected result = signal<Paged<Product> | null>(null);

  constructor() {
    combineLatest([
      this.search.valueChanges.pipe(startWith(''), debounceTime(250)),
      toObservable(this.show),
      this.page$.pipe(startWith(1)),
      this.reload$.pipe(startWith(undefined)),
    ])
      .pipe(
        // FIXME: changing the search keeps the old page number. should reset to 1
        switchMap(([search, show, page]) => this.api.search(search, show === 'all', page).pipe(catchError(() => of(null)))),
        takeUntilDestroyed(),
      )
      .subscribe((r) => this.result.set(r));
  }

  toggle(p: Product) {
    this.api.setActive(p.id, !p.isActive).subscribe((updated) => {
      toast.success(updated.isActive ? `${updated.sku} is back on sale` : `${updated.sku} discontinued`);
      this.reload$.next();
    });
  }
}
