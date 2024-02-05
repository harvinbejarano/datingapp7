import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account.service';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

export const adminGuard: CanActivateFn = (route, state) => {
  const accoutService = inject(AccountService);
  const toastr = inject(ToastrService);

  return accoutService.currentUser$.pipe(
    map(user => {
      if(!user) return false;
      if(user.roles.includes('Admin') || user.roles.includes('Moderator')){
        return true;
      } else{
        toastr.error('You cannot enter this area.');
        return false;
      }
    })
  )
};
