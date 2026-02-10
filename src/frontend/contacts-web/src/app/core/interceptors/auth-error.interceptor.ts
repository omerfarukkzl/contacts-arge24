import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authErrorInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.includes('/api/')) {
    return next(request);
  }

  const authService = inject(AuthService);
  const router = inject(Router);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && authService.isAuthenticated()) {
        void authService.logout().then(() => {
          const isAlreadyOnAuthRoute = router.url.startsWith('/auth/');
          const queryParams = isAlreadyOnAuthRoute ? {} : { returnUrl: router.url };
          void router.navigate(['/auth/login'], { queryParams });
        });
      }

      return throwError(() => error);
    })
  );
};
