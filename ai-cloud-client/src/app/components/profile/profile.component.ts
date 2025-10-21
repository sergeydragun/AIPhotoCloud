import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenService } from '../../services/token.service';
import { AuthService } from '../../services/auth.service';
import { Subscription, map } from 'rxjs';

interface DecodedToken {
  sub?: string;
  id?: string;
  name?: string;
  given_name?: string;
  email?: string;
  exp?: number;
  iat?: number;
  // добавьте здесь другие ожидаемые поля токена
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnDestroy {
  user: DecodedToken | null = null;
  private sub = new Subscription();

  constructor(
    private tokenService: TokenService,
    private jwt: JwtHelperService,
    private auth: AuthService,
    private router: Router
  ) {
    this.sub.add(
      this.tokenService.token$.pipe(
        map(token => {
          if (!token) return null;
          try {
            return this.jwt.decodeToken(token) as DecodedToken;
          } catch {
            return null;
          }
        })
      ).subscribe(decoded => {
        this.user = decoded;
      })
    );
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  isTokenExpired(): boolean {
    const t = this.tokenService.getToken();
    if (!t) return true;
    return this.jwt.isTokenExpired(t);
  }

  get userId(): string {
    return this.user?.sub ?? this.user?.id ?? '—';
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }
}
