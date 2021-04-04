import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
@Injectable({
  providedIn: 'root'
})
export class AccountService {

  baseUrl=environment.apiUrl;
  private currentUserSource=new ReplaySubject<User>(1);
  currentUser$=this.currentUserSource.asObservable();

  constructor(private http:HttpClient) { }

  login(model:any){
    return this.http.post(this.baseUrl+'account/login',model).pipe(
      map((response:User)=>{
        const user=response;
        if(user){
          //localStorage.setItem('user',JSON.stringify(user));
          //this.currentUserSource.next(user);

          this.setCurrentUser(user);
        }
      })
    );
  }

  //  Register Method
  register(model:any){
    return this.http.post(this.baseUrl+'account/register',model).pipe(
      map((user:User)=>{ // Assigning user with returned value from register endoint
        if(user){
          // localStorage.setItem('user',JSON.stringify(user));  //  setting localStorage with the returned response from register endpoint
          //this.currentUserSource.next(user); //assigning currentSource property with returned value i.e user

          this.setCurrentUser(user);
        }      
      })
    )
  }

  setCurrentUser(user:User){
    localStorage.setItem('user',JSON.stringify(user));  //  setting localStorage with the returned response from register endpoint
    this.currentUserSource.next(user);
  }
  logout(){
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
