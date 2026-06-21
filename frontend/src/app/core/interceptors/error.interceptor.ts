import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Don't show toast for 401 Unauthorized (Auth guard/interceptor usually handles this)
      // Or 400 Bad Request if it's handled by a specific component (like login)
      
      // We can skip specific URLs if needed, but for global errors like 500, timeouts, etc.:
      if (error.status === 0 || error.status >= 500) {
        toastService.error('A network or server error occurred. Please try again.');
      } else if (error.status === 403) {
        toastService.error('You do not have permission to perform this action.');
      } else if (error.status === 404) {
        // Sometimes 404 is expected (like searching for a movie not in DB). Use carefully.
      } else if (error.status !== 401 && error.status !== 400) {
        // Fallback for other errors not explicitly handled by components
        const message = error.error?.message || error.message || 'An unexpected error occurred.';
        toastService.error(message);
      }

      return throwError(() => error);
    })
  );
};
