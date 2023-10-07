import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account.service';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  const accoutService = inject(AccountService);
  const taostr = inject(ToastrService);
  
  return accoutService.currentUser$.pipe(
    map(user =>{
      if(user) return true;
      else{
        taostr.error('You shall not pass!');
        return false;
      }
    })
  )
};
