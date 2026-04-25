import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { toast } from '@spartan-ng/brain/sonner';
import { catchError, throwError } from 'rxjs';
import { ApiError } from './api-error';

// Turns http errors into ApiError and pops a toast.
// 400s with field errors are skipped - the form shows those next to the inputs instead.
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((err: unknown) => {
      if (!(err instanceof HttpErrorResponse)) return throwError(() => err);

      const error = ApiError.from(err);
      if (!error.isValidation) toast.error(error.message);
      // TODO: 401 -> redirect to login, once there is a login
      return throwError(() => error);
    }),
  );
