import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { toast } from '@spartan-ng/brain/sonner';
import { ApiError } from './api-error';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let ctrl: HttpTestingController;
  let toastError: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([errorInterceptor])), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpClient);
    ctrl = TestBed.inject(HttpTestingController);
    toastError = vi.spyOn(toast, 'error').mockImplementation(() => 1);
  });

  afterEach(() => vi.restoreAllMocks());

  it('turns a problem response into an ApiError and toasts the detail', () => {
    let caught: ApiError | undefined;
    http.get('/api/x').subscribe({ error: (e) => (caught = e) });

    ctrl.expectOne('/api/x').flush({ title: 'Conflict', detail: 'Already has a signed price.' }, { status: 409, statusText: 'Conflict' });

    expect(caught).toBeInstanceOf(ApiError);
    expect(caught?.status).toBe(409);
    expect(toastError).toHaveBeenCalledWith('Already has a signed price.');
  });

  it('does not toast field validation errors (the form shows them)', () => {
    let caught: ApiError | undefined;
    http.post('/api/x', {}).subscribe({ error: (e) => (caught = e) });

    ctrl.expectOne('/api/x').flush({ errors: { sku: ['Taken'] } }, { status: 400, statusText: 'Bad Request' });

    expect(caught?.isValidation).toBe(true);
    expect(caught?.fieldErrors['sku']).toEqual(['Taken']);
    expect(toastError).not.toHaveBeenCalled();
  });

  it('gives a readable message when the server is unreachable', () => {
    let caught: ApiError | undefined;
    http.get('/api/x').subscribe({ error: (e) => (caught = e) });

    ctrl.expectOne('/api/x').error(new ProgressEvent('error'), { status: 0 });

    expect(caught?.message).toContain("Can't reach the server");
  });
});
