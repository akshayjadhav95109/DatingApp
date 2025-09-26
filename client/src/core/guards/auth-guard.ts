import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { ToastService } from '../services/toast-service';
import { AccountService } from '../services/account-service';

export const authGuard: CanActivateFn = () => {
  const accountServices = inject(AccountService);
  const toast = inject(ToastService);

  if (accountServices.currentUser()) return true;
  else {
    toast.error('You shall not pass');
    return false;
  }
};
