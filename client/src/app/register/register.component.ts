import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit{
  @Output() cancelRegister = new EventEmitter();

  model: any = {};

  constructor(
    private accoutService: AccountService,
    private toastr: ToastrService) {}

  ngOnInit(): void {
  }

  register(){
     this.accoutService.register(this.model).subscribe({
      next: () => {
        this.cancel();
      },
      error: error => this.toastr.error(error.error.title)
     })
  }

  cancel(){
    this.cancelRegister.emit(false);
  }

}
