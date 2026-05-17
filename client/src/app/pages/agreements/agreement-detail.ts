import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { UnitPipe } from '../../shared/unit.pipe';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmAlertDialogImports } from '@spartan-ng/helm/alert-dialog';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';
import { AgreementsApi } from '../../core/api';
import { Agreement } from '../../core/models';
import { StatusBadge } from '../../shared/status-badge';

@Component({
  selector: 'app-agreement-detail',
  imports: [RouterLink, ReactiveFormsModule, CurrencyPipe, DatePipe, HlmButtonImports, HlmAlertDialogImports, HlmTextareaImports, HlmLabelImports, StatusBadge, DecimalPipe, UnitPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './agreement-detail.html',
})
export class AgreementDetail implements OnInit {
  id = input.required<string>();

  private api = inject(AgreementsApi);

  protected agreement = signal<Agreement | null>(null);
  protected busy = signal(false);
  protected reason = new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(500)] });
  protected reasonMissing = signal(false); // zoneless: plain control.touched doesn't re-render, so use a signal
  protected canCancel = computed(() => ['Draft', 'Active'].includes(this.agreement()?.status ?? ''));

  ngOnInit() {
    this.api.get(this.id()).subscribe((a) => this.agreement.set(a));
  }

  sign() {
    this.busy.set(true);
    this.api.sign(this.id()).subscribe({
      next: (a) => this.updated(a, 'Agreement signed'),
      error: () => this.busy.set(false), // interceptor shows the message (e.g. overlapping agreement)
    });
  }

  cancel(close: () => void) {
    if (this.reason.invalid) {
      this.reasonMissing.set(true);
      return;
    }
    this.reasonMissing.set(false);
    this.busy.set(true);
    this.api.cancel(this.id(), this.reason.value.trim()).subscribe({
      next: (a) => {
        close();
        this.updated(a, 'Agreement cancelled');
      },
      error: () => this.busy.set(false),
    });
  }

  private updated(a: Agreement, message: string) {
    this.agreement.set(a);
    this.busy.set(false);
    toast.success(message);
  }
}
