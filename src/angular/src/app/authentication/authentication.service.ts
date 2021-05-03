import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { map } from 'rxjs/operators';
import { LoginResponse } from './login-result.model';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  constructor(private http: HttpClient) {}

  public isLoggedIn(): boolean {
    return !!localStorage.getItem('auth_id');
  }

  public login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`/Api/Users/Login`, {
        email,
        password,
      })
      .pipe(
        map((result) => {
          if (result && result.id) {
            localStorage.setItem('auth_name', result.name);
            localStorage.setItem('auth_id', result.id);
          }
          return result;
        })
      );
  }

  public logout(): Observable<void> {
    localStorage.removeItem('auth_id');
    localStorage.removeItem('auth_name');
    localStorage.clear();

    return this.http.get<void>('/Api/Users/Logout');
  }

  public resetPasswordRequest(email: string): Observable<void> {
    return this.http.post<void>(`/Api/Users/ResetPasswordRequest`, {
      email,
    });
  }

  public resetPassword(userId: string, token: string, password1: string, password2: string): Observable<void> {
    return this.http.post<void>(`/Api/Users/ResetPassword`, {
      userId,
      token,
      password1,
      password2,
    });
  }
}
