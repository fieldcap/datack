import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from '../models/api-error.model';

export class ErrorHelper {
  public static format(error: HttpErrorResponse | Error | string): string {
    if (typeof error === 'string') {
      return error;
    } else if (error instanceof HttpErrorResponse) {
      if (error.status === 404) {
        return `The URL ${error.url} was not found`;
      } else if (error.status === 401) {
        return `Access was denied to URL ${error.url}`;
      } else if (error.status === 500) {
        if (this.isApiError(error.error)) {
          return this.getInner(error.error);
        } else {
          return error.message;
        }
      } else if (error.error) {
        if (error.error.errors) {
          const values = Object.values(error.error.errors);
          return values.join('<br/>');
        }
        return error.error;
      }
      return error.message;
    } else if (error instanceof Error) {
      return error.message;
    }
  }

  public static isApiError(error: ApiError | HttpErrorResponse): error is ApiError {
    return (error as ApiError).exception != null;
  }

  private static getInner(error: ApiError): string {
    if (error.innerExceptions && error.innerExceptions.length > 0) {
      return this.getInner(error.innerExceptions[0]);
    }
    return error.exception;
  }
}
