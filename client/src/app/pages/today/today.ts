import { CurrencyPipe, DatePipe, I18nPluralPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { forkJoin } from 'rxjs';
import { AgreementsApi } from '../../core/api';
import { Dashboard, Renewal } from '../../core/models';
import { DockBoard } from '../../shared/dock-board';

@Component({
  selector: 'app-today',
  imports: [RouterLink, CurrencyPipe, DatePipe, I18nPluralPipe, DockBoard, HlmSkeletonImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './today.html',
})
export class Today {
  private api = inject(AgreementsApi);

  protected readonly now = new Date();
  protected summary = signal<Dashboard | null>(null);
  protected renewals = signal<Renewal[] | null>(null);
  protected failed = signal(false);

  protected readonly plural = {
    customer: { '=1': '1 customer', other: '# customers' },
    product: { '=1': '1 product', other: '# products' },
    agreement: { '=0': 'No signed agreements are', '=1': '1 signed agreement is', other: '# signed agreements are' },
    draft: { '=0': 'No quotes are', '=1': '1 quote is', other: '# quotes are' },
  };

  constructor() {
    forkJoin({ summary: this.api.dashboard(), renewals: this.api.renewals() })
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: ({ summary, renewals }) => {
          this.summary.set(summary);
          this.renewals.set(renewals);
        },
        error: () => this.failed.set(true),
      });
  }
}
