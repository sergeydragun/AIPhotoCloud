import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, RegisterDto } from '../../services/auth.service';
import { catchError, finalize, of } from 'rxjs';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  form: FormGroup;
  loading = false;
  error: string | null = null;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.email]],
      phoneNumber: [''],
      userName: [''],
      name: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordsMatch });
  }

  passwordsMatch(group: AbstractControl) {
    const p = group.get('password')?.value;
    const c = group.get('confirmPassword')?.value;
    return p === c ? null : { passwordsMismatch: true };
  }

  onSubmit() {
    this.error = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const email = this.form.value.email || null;
    const username = this.form.value.userName || null;
    const phone = this.form.value.phoneNumber || null;
    const name = this.form.value.name as string;

    if (!(email || username || phone)) {
      this.error = 'Нужно указать email или логин или телефон';
      return;
    }

    this.loading = true;

    const dto: RegisterDto = {
      email,
      userName: username,
      phoneNumber: phone,
      name,
      password: this.form.value.password!,
      confirmPassword: this.form.value.confirmPassword!
    };

    this.auth.register(dto).pipe(
      catchError(err => {
        this.error = err?.error?.error ?? 'Ошибка регистрации';
        return of(null);
      }),
      finalize(() => this.loading = false)
    ).subscribe(res => {
      if (!res) return;
      if (res.success) {
        this.router.navigate(['/']);
      } else {
        this.error = res.error ?? 'Не удалось зарегистрироваться';
      }
    });
  }
}
