import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { inject } from '@angular/core';

export const videoGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.getCurrentUser()) {
    return true; // 已登入，允許進入
  } else {
    router.navigate(['/login']); // 未登入，導向登入頁
    return false;
  }
};
