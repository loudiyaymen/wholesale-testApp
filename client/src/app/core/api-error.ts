import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from './models';

export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
    readonly fieldErrors: Record<string, string[]> = {},
  ) {
    super(message);
  }

  static from(res: HttpErrorResponse): ApiError {
    // status 0 = request never got a response (server down, CORS, offline...)
    if (res.status === 0) return new ApiError(0, "Can't reach the server. Check that the API is running.");

    const problem = (res.error ?? {}) as ProblemDetails;
    return new ApiError(res.status, problem.detail ?? problem.title ?? `Request failed (${res.status})`, problem.errors ?? {});
  }

  get isValidation() {
    return this.status === 400 && Object.keys(this.fieldErrors).length > 0;
  }
}
