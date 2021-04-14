import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();  
  registerForm: FormGroup; //reactive form
  maxDate:Date;
  validationErrors:string[]=[];

  constructor(private accountService: AccountService, 
    private toastr: ToastrService,
    private fb:FormBuilder,
    private router:Router) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate=new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() -18);

  }

  //initializing form with valiations
  initializeForm() {
    this.registerForm = this.fb.group({
      gender: ['male'], //username form control with validations
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required,
      Validators.minLength(4),
      Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required,this.matchValues('password')]]
    })

    this.registerForm.controls.password.valueChanges.subscribe(()=>{ //it will revalidate if we update value in password field
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    })
  }

  matchValues(matchTo: string): ValidatorFn { //method is being used for confirmPassword control
    return (control: AbstractControl) => {
      return control?.value === control?.parent?.controls[matchTo].value 
      ? null : { isMatching: true }  //isMatching is object like for minLength is minlength
    }
  }

  register() {    
    this.accountService.register(this.registerForm.value).subscribe(response=>{
      this.router.navigateByUrl('/members');
      this.cancel();
    },error=>{
      this.validationErrors=error;
      
    })
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

}
