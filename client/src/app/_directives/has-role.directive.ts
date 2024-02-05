import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { User } from '../models/user';
import { AccountService } from '../services/account.service';
import { take } from 'rxjs';

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[] = [];
  user: User = {} as User;

  constructor( private viewContainerRef: ViewContainerRef, private templateref: TemplateRef<any>,
    private accoutService: AccountService) {
      this.accoutService.currentUser$.pipe(take(1)).subscribe({
        next: user => {
          if(user) this.user = user
        }
      })
     }

  ngOnInit(): void {
    if(this.user.roles.some(r => this.appHasRole.includes(r))){
      this.viewContainerRef.createEmbeddedView(this.templateref);
    }else {
      this.viewContainerRef.clear();
    }
  }

}
