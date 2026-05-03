import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { AgreementStatus } from '../core/models';

@Component({
  selector: 'app-status-badge',
  imports: [HlmBadgeImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span hlmBadge [variant]="status() === 'Draft' ? 'outline' : 'default'" [class]="tone()">{{ label() }}</span>`,
})
export class StatusBadge {
  status = input.required<AgreementStatus>();

  protected label = computed(() => (this.status() === 'Active' ? 'Signed' : this.status()));

  protected tone = computed(() => {
    switch (this.status()) {
      case 'Active':
        return 'bg-primary/12 text-primary';
      case 'Expired':
        return 'bg-signal/40 text-foreground';
      case 'Cancelled':
        return 'bg-muted text-muted-foreground line-through';
      default:
        return '';
    }
  });
}
