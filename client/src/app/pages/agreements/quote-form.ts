import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { UnitPipe } from '../../shared/unit.pipe';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { AgreementsApi, CustomersApi, ProductsApi } from '../../core/api';
import { Customer, Product, TERMS } from '../../core/models';
import { applyServerErrors, notInPast, serverError, todayIso } from '../../shared/forms';
import { annualValue, checkPrice, discountPercent, priceFloor } from '../../shared/pricing';

@Component({
  selector: 'app-quote-form',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DecimalPipe, HlmButtonImports, HlmFieldImports, HlmInputImports, HlmSelectImports, UnitPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './quote-form.html',
})
export class QuoteForm implements OnInit {
  // all optional, filled in by "Quote renewal" / "Quote a price" links
  customerId = input<string>();
  productId = input<string>();
  startDate = input<string>();
  unitPrice = input<string>();
  annualVolume = input<string>();

  private customersApi = inject(CustomersApi);
  private productsApi = inject(ProductsApi);
  private agreementsApi = inject(AgreementsApi);
  private router = inject(Router);

  protected readonly terms = TERMS;
  protected readonly today = todayIso();
  protected customers = signal<Customer[]>([]);
  protected products = signal<Product[]>([]);
  protected saving = signal(false);
  protected serverError = serverError;

  protected form = inject(FormBuilder).nonNullable.group({
    customerId: ['', Validators.required],
    productId: ['', Validators.required],
    unitPrice: [null as number | null, [Validators.required, Validators.min(0.01)]],
    annualVolume: [null as number | null, [Validators.required, Validators.min(1)]],
    termMonths: [12, Validators.required],
    startDate: [this.today, [Validators.required, notInPast()]],
  });

  private value = toSignal(this.form.valueChanges, { initialValue: this.form.getRawValue() });

  protected product = computed(() => this.products().find((p) => p.id === this.value().productId) ?? null);
  protected price = computed(() => Number(this.value().unitPrice) || 0);
  protected yearly = computed(() => annualValue(Number(this.value().annualVolume) || 0, this.price()));
  protected discount = computed(() => discountPercent(this.product()?.listPrice ?? 0, this.price()));
  protected floor = computed(() => priceFloor(this.product()?.listPrice ?? 0));
  protected priceProblem = computed(() => checkPrice(this.product()?.listPrice ?? 0, this.price()));

  protected termLabel = (months: number) => `${months} months`;
  protected customerName = (id: string) => this.customers().find((c) => c.id === id)?.legalName ?? '';
  protected productName = (id: string) => {
    const p = this.products().find((x) => x.id === id);
    return p ? `${p.sku}  ${p.name}` : '';
  };

  ngOnInit() {
    // TODO: these only load the first 100. needs a searchable combobox once there are more customers
    this.customersApi.search('', 1, 100).subscribe((r) => this.customers.set(r.items));
    this.productsApi.search('', false, 1, 100).subscribe((r) => this.products.set(r.items));

    const start = this.startDate();
    this.form.patchValue({
      customerId: this.customerId() ?? '',
      productId: this.productId() ?? '',
      unitPrice: this.unitPrice() ? Number(this.unitPrice()) : null,
      annualVolume: this.annualVolume() ? Number(this.annualVolume()) : null,
      // edge case: renewal link for an agreement that already ended would give a past start date
      startDate: start && start >= this.today ? start : this.today,
    });
  }

  save() {
    if (this.form.invalid || this.priceProblem()) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.saving.set(true);
    this.agreementsApi
      .quote({ ...v, unitPrice: Number(v.unitPrice), annualVolume: Number(v.annualVolume) })
      .subscribe({
        next: (a) => {
          toast.success('Quote saved');
          this.router.navigate(['/agreements', a.id]);
        },
        error: (err) => {
          applyServerErrors(this.form, err);
          this.saving.set(false);
        },
      });
  }
}
