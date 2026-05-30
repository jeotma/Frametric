import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent {
  private readonly _fb = inject(FormBuilder);
  private readonly _auth = inject(AuthService);
  private readonly _router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  readonly form = this._fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  get emailCtrl() { return this.form.get('email')!; }
  get passwordCtrl() { return this.form.get('password')!; }

  togglePassword() { this.showPassword.update(v => !v); }

  submit() {
    if (this.form.invalid || this.isLoading()) return;
    this.errorMessage.set(null);
    this.isLoading.set(true);

    const { email, password } = this.form.value;

    this._auth.login(email!, password!).subscribe({
      next: () => {
        this.isLoading.set(false);
        this._router.navigate(['/']);
      },
      error: (err) => {
        this.isLoading.set(false);
        const status = err?.status;
        if (status === 401 || status === 400) {
          const backendMsg = typeof err?.error === 'string' ? err.error : err?.error?.message;
          this.errorMessage.set(backendMsg || 'Invalid email or password. Please try again.');
        } else {
          this.errorMessage.set('Something went wrong. Please try again later.');
        }
      },
    });
  }
}
