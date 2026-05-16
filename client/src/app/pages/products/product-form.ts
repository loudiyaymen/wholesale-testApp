import { ChangeDetectionStrategy, Component, OnInit, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { ProductsApi } from '../../core/api';
import { UNITS, UnitOfMeasure } from '../../core/models';
import { applyServerErrors, serverError } from '../../shared/forms';

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule, RouterLink, HlmButtonImports, HlmFieldImports, HlmInputImports, HlmSelectImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1 class="mb-6 text-4xl font-bold">{{ id() ? 'Edit product' : 'Add product' }}</h1>

    <form [formGroup]="form" (ngSubmit)="save()" novalidate class="border-border max-w-3xl border bg-card p-6">
      <hlm-field-group class="grid gap-6 sm:grid-cols-2">
        <hlm-field>
          <label hlmFieldLabel for="sku">SKU</label>
          <input hlmInput id="sku" formControlName="sku" class="uppercase" />
          @if (id()) { <hlm-field-description>SKU and unit can't change once agreements may use them.</hlm-field-description> }
          <hlm-field-error validator="required">Enter a SKU.</hlm-field-error>
          <hlm-field-error validator="pattern">Letters, numbers and dashes only.</hlm-field-error>
          <hlm-field-error validator="server">{{ serverError(form.controls.sku) }}</hlm-field-error>
        </hlm-field>

        <hlm-field>
          <label hlmFieldLabel for="unit">Sold by the</label>
          <hlm-select formControlName="unit">
            <hlm-select-trigger buttonId="unit" class="w-full"><hlm-select-value /></hlm-select-trigger>
            <hlm-select-content *hlmSelectPortal>
              @for (u of units; track u) { <hlm-select-item [value]="u">{{ u }}</hlm-select-item> }
            </hlm-select-content>
          </hlm-select>
        </hlm-field>

        <hlm-field class="sm:col-span-2">
          <label hlmFieldLabel for="name">Name</label>
          <input hlmInput id="name" formControlName="name" />
          <hlm-field-error validator="required">Enter a product name.</hlm-field-error>
        </hlm-field>

        <hlm-field>
          <label hlmFieldLabel for="category">Category</label>
          <input hlmInput id="category" formControlName="category" />
          <!-- TODO: should be a dropdown of existing categories, right now typos make new ones -->
          <hlm-field-error validator="required">Enter a category.</hlm-field-error>
        </hlm-field>

        <hlm-field>
          <label hlmFieldLabel for="listPrice">List price ($ per unit)</label>
          <input hlmInput id="listPrice" type="number" step="0.01" min="0.01" formControlName="listPrice" />
          <hlm-field-error validator="required">Enter a list price.</hlm-field-error>
          <hlm-field-error validator="min">List price has to be more than $0.</hlm-field-error>
          <hlm-field-error validator="server">{{ serverError(form.controls.listPrice) }}</hlm-field-error>
        </hlm-field>
      </hlm-field-group>

      <div class="mt-8 flex justify-end gap-2">
        <a hlmBtn variant="outline" routerLink="/products">Cancel</a>
        <button hlmBtn type="submit" [disabled]="saving()">{{ saving() ? 'Saving…' : id() ? 'Save changes' : 'Add product' }}</button>
      </div>
    </form>
  `,
})
export class ProductForm implements OnInit {
  id = input<string>();

  private api = inject(ProductsApi);
  private router = inject(Router);

  protected readonly units = UNITS;
  protected saving = signal(false);
  protected serverError = serverError;

  protected form = inject(FormBuilder).nonNullable.group({
    sku: ['', [Validators.required, Validators.maxLength(40), Validators.pattern(/^[A-Za-z0-9-]+$/)]],
    unit: ['Case' as UnitOfMeasure, Validators.required],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    category: ['', [Validators.required, Validators.maxLength(80)]],
    listPrice: [null as number | null, [Validators.required, Validators.min(0.01)]],
  });

  ngOnInit() {
    const id = this.id();
    if (!id) return;
    this.form.controls.sku.disable();
    this.form.controls.unit.disable();
    this.api.get(id).subscribe((p) => this.form.patchValue(p));
  }

  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const id = this.id();
    const req = id
      ? this.api.update(id, { name: v.name, category: v.category, listPrice: Number(v.listPrice) })
      : this.api.create({ sku: v.sku, unit: v.unit, name: v.name, category: v.category, listPrice: Number(v.listPrice) });

    this.saving.set(true);
    req.subscribe({
      next: (p) => {
        toast.success(id ? `${p.sku} saved` : `${p.sku} added`);
        this.router.navigate(['/products']);
      },
      error: (err) => {
        applyServerErrors(this.form, err);
        this.saving.set(false);
      },
    });
  }
}
