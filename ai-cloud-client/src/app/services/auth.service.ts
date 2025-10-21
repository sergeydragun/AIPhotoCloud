import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { TokenService } from './token.service';

export interface LoginDto {
  identifier: string;
  password: string;
  identifierType?: 'Auto'|'Email'|'UserName'|'Phone';
  rememberMe?: boolean;
}

export interface RegisterDto {
  email?: string|null;
  userName?: string|null;
  phoneNumber?: string|null;
  name: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResult {
  success: boolean;
  token?: string;
  expires?: string;
  error?: string;
  userId?: string;
  email?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private base = '/api/auth';

  constructor(private http: HttpClient, private tokenService: TokenService) {}

  login(dto: LoginDto): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.base}/login`, dto).pipe(
      tap(res => {
        if (res?.success && res.token) {
          this.tokenService.setToken(res.token, !!dto.rememberMe);
        }
      })
    );
  }

  register(dto: RegisterDto): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.base}/register`, dto).pipe(
      tap(res => {
        if (res?.success && res.token) {
          this.tokenService.setToken(res.token);
        }
      })
    );
  }

  logout(): void {
    this.tokenService.clear();
  }

  isLoggedIn(): boolean {
    return !this.tokenService.isTokenExpired();
  }
}
