import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AgreementsApi, ProductsApi, toParams } from './api';

describe('toParams', () => {
  it('drops empty values', () => {
    const p = toParams({ a: 'x', b: '', c: null, d: undefined, e: 0, f: false });
    expect(p.keys()).toEqual(['a', 'e', 'f']);
  });
});

describe('api services', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('only sends the agreement filters that are set', () => {
    TestBed.inject(AgreementsApi).list({ status: 'Draft', customerId: null, page: 2 }).subscribe();

    const req = http.expectOne((r) => r.url === '/api/agreements');
    expect(req.request.params.get('status')).toBe('Draft');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.has('customerId')).toBe(false);
    req.flush({ items: [], page: 2, pageSize: 20, totalCount: 0, totalPages: 0 });
  });

  it('signs via POST /sign', () => {
    TestBed.inject(AgreementsApi).sign('abc').subscribe();

    const req = http.expectOne('/api/agreements/abc/sign');
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('discontinue and reinstate hit different endpoints', () => {
    const api = TestBed.inject(ProductsApi);
    api.setActive('p1', false).subscribe();
    api.setActive('p1', true).subscribe();

    http.expectOne('/api/products/p1/discontinue').flush({});
    http.expectOne('/api/products/p1/reinstate').flush({});
  });

  it('leaves includeDiscontinued off the url when false', () => {
    TestBed.inject(ProductsApi).search('', false).subscribe();

    const req = http.expectOne((r) => r.url === '/api/products');
    expect(req.request.params.has('includeDiscontinued')).toBe(false);
    req.flush({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
  });
});
