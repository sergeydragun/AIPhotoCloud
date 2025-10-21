import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly STORAGE_KEY = 'kto_prochital_tot_sdohnet';
  private tokenSub = new BehaviorSubject<string | null>(this.readTokenFromStorage());
  public token$ = this.tokenSub.asObservable();

  constructor(private jwt: JwtHelperService) {}

  private readTokenFromStorage(): string | null {
    return localStorage.getItem(this.STORAGE_KEY);
  }

  getToken(): string | null {
    return this.tokenSub.getValue();
  }

  setToken(token: string | null, remember = true) {
    this.tokenSub.next(token);
    if (token && remember) {
      localStorage.setItem(this.STORAGE_KEY, token);
    } else if (!token) {
      localStorage.removeItem(this.STORAGE_KEY);
    } else {
      localStorage.removeItem(this.STORAGE_KEY);
    }
  }

  clear() {
    this.setToken(null);
  }

  isTokenExpired(): boolean {
    const t = this.getToken();
    if (!t) return true;
    return this.jwt.isTokenExpired(t);
  }

  isLoggedIn$(): Observable<boolean> {
    return new Observable(observer => {
      const sub = this.token$.subscribe(token => {
        observer.next(!!token && !this.jwt.isTokenExpired(token));
      });
      return () => sub.unsubscribe();
    });
  }
}
