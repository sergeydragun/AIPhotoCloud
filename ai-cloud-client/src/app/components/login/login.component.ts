import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { catchError, finalize, of } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  error: string | null = null;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.form = this.fb.group({
      identifier: ['', [Validators.required]],
      password: ['', [Validators.required]],
      rememberMe: [true]
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = null;

    const dto = {
      identifier: this.form.value.identifier,
      password: this.form.value.password,
      identifierType: 'Auto' as const,
      rememberMe: this.form.value.rememberMe
    };

    this.auth.login(dto).pipe(
      catchError(err => {
        this.error = err?.error?.error ?? 'Ошибка входа';
        return of(null);
      }),
      finalize(() => this.loading = false)
    ).subscribe(res => {
      if (!res) return;
      if (res.success) {
        this.router.navigate(['/']);
      } else {
        this.error = res.error ?? 'Не удалось войти';
      }
    });
  }
}
