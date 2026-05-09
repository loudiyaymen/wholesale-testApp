import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { HlmTabsImports } from '@spartan-ng/helm/tabs';
import { catchError, of, switchMap, tap } from 'rxjs';
import { AgreementsApi } from '../../core/api';
import { Renewal } from '../../core/models';
import { DockBoard } from '../../shared/dock-board';

@Component({
  selector: 'app-renewals',
  imports: [HlmTabsImports, DockBoard],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1 class="mb-2 text-4xl font-bold">Renewals</h1>
    <p class="text-muted-foreground mb-6 max-w-2xl">Signed agreements running out soon. Call these customers before their price goes back to list.</p>

    <hlm-tabs [tab]="days()" (tabActivated)="days.set($any($event))" class="mb-4">
      <hlm-tabs-list aria-label="Ending within">
        @for (d of windows; track d) { <button [hlmTabsTrigger]="d">{{ d }} days</button> }
      </hlm-tabs-list>
    </hlm-tabs>

    @if (renewals(); as list) {
      @if (list.length) {
        <app-dock-board [renewals]="list" />
      } @else {
        <p class="text-muted-foreground border-border border-y bg-card px-5 py-6">Nothing ends in the next {{ days() }} days.</p>
      }
    } @else {
      <p class="text-muted-foreground">Loading…</p>
    }
  `,
})
export class Renewals {
  private api = inject(AgreementsApi);

  protected readonly windows = ['30', '60', '90', '180'];
  protected days = signal('60');
  protected renewals = signal<Renewal[] | null>(null);

  constructor() {
    toObservable(this.days)
      .pipe(
        tap(() => this.renewals.set(null)),
        switchMap((d) => this.api.renewals(Number(d)).pipe(catchError(() => of([])))),
        takeUntilDestroyed(),
      )
      .subscribe((r) => this.renewals.set(r));
  }
}
