import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';

@Component({
  selector: 'app-pager',
  imports: [HlmButtonImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (totalPages() > 1) {
      <nav class="mt-4 flex items-center justify-end gap-3 text-sm" aria-label="Pages">
        <button hlmBtn variant="outline" size="sm" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)">Previous</button>
        <span class="text-muted-foreground tabular-nums">{{ page() }} of {{ totalPages() }}</span>
        <button hlmBtn variant="outline" size="sm" [disabled]="page() >= totalPages()" (click)="pageChange.emit(page() + 1)">Next</button>
      </nav>
    }
  `,
})
export class Pager {
  page = input.required<number>();
  totalPages = input.required<number>();
  pageChange = output<number>();
}
