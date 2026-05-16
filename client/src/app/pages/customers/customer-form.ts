import { ChangeDetectionStrategy, Component, OnInit, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { CustomersApi } from '../../core/api';
import { PAYMENT_TERMS, PaymentTerms } from '../../core/models';
import { applyServerErrors, serverError } from '../../shared/forms';

// used for both /customers/new and /customers/:id/edit
@Component({
  selector: 'app-customer-form',
  imports: [ReactiveFormsModule, RouterLink, HlmButtonImports, HlmFieldImports, HlmInputImports, HlmSelectImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './customer-form.html',
})
export class CustomerForm implements OnInit {
  id = input<string>();

  private api = inject(CustomersApi);
  private router = inject(Router);

  protected readonly terms = PAYMENT_TERMS;
  protected saving = signal(false);
  protected serverError = serverError;

  protected form = inject(FormBuilder).nonNullable.group({
    legalName: ['', [Validators.required, Validators.maxLength(200)]],
    businessNumber: ['', [Validators.required, Validators.pattern(/^\d{9}$/)]],
    contactName: ['', [Validators.required, Validators.maxLength(120)]],
    contactEmail: ['', [Validators.required, Validators.email]],
    contactPhone: [''],
    shippingAddress: ['', [Validators.required, Validators.maxLength(300)]],
    paymentTerms: ['Net30' as PaymentTerms, Validators.required],
  });

  ngOnInit() {
    const id = this.id();
    if (id) this.api.get(id).subscribe((c) => this.form.patchValue({ ...c, contactPhone: c.contactPhone ?? '' }));
  }

  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const body = { ...value, contactPhone: value.contactPhone.trim() || null };
    const id = this.id();

    this.saving.set(true);
    (id ? this.api.update(id, body) : this.api.create(body)).subscribe({
      next: (c) => {
        toast.success(id ? 'Customer saved' : `${c.legalName} added`);
        this.router.navigate(['/customers', c.id]);
      },
      error: (err) => {
        applyServerErrors(this.form, err);
        this.saving.set(false);
      },
    });
  }
}
