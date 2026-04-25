import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import {
  Agreement,
  AgreementStatus,
  Customer,
  CustomerRequest,
  Dashboard,
  NewAgreement,
  Paged,
  Product,
  Renewal,
  UnitOfMeasure,
} from './models';

// skips null/undefined/'' so we don't send ?status=&customerId=
export function toParams(values: Record<string, string | number | boolean | null | undefined>): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(values)) {
    if (value !== null && value !== undefined && value !== '') params = params.set(key, String(value));
  }
  return params;
}

// one service per resource felt like overkill for this size, so they all live here.
// split it up if it grows

@Injectable({ providedIn: 'root' })
export class CustomersApi {
  private http = inject(HttpClient);

  search(search = '', page = 1, pageSize = 20) {
    return this.http.get<Paged<Customer>>('/api/customers', { params: toParams({ search: search.trim(), page, pageSize }) });
  }
  get(id: string) {
    return this.http.get<Customer>(`/api/customers/${id}`);
  }
  create(body: CustomerRequest) {
    return this.http.post<Customer>('/api/customers', body);
  }
  update(id: string, body: CustomerRequest) {
    return this.http.put<Customer>(`/api/customers/${id}`, body);
  }
}

@Injectable({ providedIn: 'root' })
export class ProductsApi {
  private http = inject(HttpClient);

  search(search = '', includeDiscontinued = false, page = 1, pageSize = 20) {
    return this.http.get<Paged<Product>>('/api/products', {
      params: toParams({ search: search.trim(), includeDiscontinued: includeDiscontinued || null, page, pageSize }),
    });
  }
  get(id: string) {
    return this.http.get<Product>(`/api/products/${id}`);
  }
  create(body: { sku: string; name: string; category: string; unit: UnitOfMeasure; listPrice: number }) {
    return this.http.post<Product>('/api/products', body);
  }
  update(id: string, body: { name: string; category: string; listPrice: number }) {
    return this.http.put<Product>(`/api/products/${id}`, body);
  }
  setActive(id: string, active: boolean) {
    return this.http.post<Product>(`/api/products/${id}/${active ? 'reinstate' : 'discontinue'}`, null);
  }
}

@Injectable({ providedIn: 'root' })
export class AgreementsApi {
  private http = inject(HttpClient);

  list(filter: { status?: AgreementStatus | null; customerId?: string | null; productId?: string | null; page?: number; pageSize?: number } = {}) {
    return this.http.get<Paged<Agreement>>('/api/agreements', { params: toParams({ ...filter }) });
  }
  get(id: string) {
    return this.http.get<Agreement>(`/api/agreements/${id}`);
  }
  quote(body: NewAgreement) {
    return this.http.post<Agreement>('/api/agreements', body);
  }
  sign(id: string) {
    return this.http.post<Agreement>(`/api/agreements/${id}/sign`, null);
  }
  cancel(id: string, reason: string) {
    return this.http.post<Agreement>(`/api/agreements/${id}/cancel`, { reason });
  }
  renewals(withinDays = 60) {
    return this.http.get<Renewal[]>('/api/agreements/renewals', { params: toParams({ withinDays }) });
  }
  dashboard() {
    return this.http.get<Dashboard>('/api/dashboard');
  }
}
