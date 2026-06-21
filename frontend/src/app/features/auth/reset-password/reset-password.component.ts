import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss'],
})
export class ResetPasswordComponent implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _auth = inject(AuthService);
  private readonly _router = inject(Router);
  private readonly _route = inject(ActivatedRoute);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly showPassword = signal(false);
  readonly loadingMessage = signal<string>('Resetting...');

  token: string | null = null;
  email: string | null = null;

  readonly form = this._fb.group({
    password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/)]],
  });

  get passwordCtrl() { return this.form.get('password')!; }

  ngOnInit(): void {
    this._route.queryParams.subscribe(params => {
      this.token = params['token'];
      this.email = params['email'];

      if (!this.token || !this.email) {
        this.errorMessage.set('Invalid password reset link. Please request a new one.');
      }
    });
  }

  togglePassword() { this.showPassword.update(v => !v); }

  submit() {
    if (this.form.invalid || this.isLoading() || !this.token || !this.email) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.isLoading.set(true);

    const { password } = this.form.value;

    this._auth.resetPassword(this.email, this.token, password!).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.successMessage.set('Password has been successfully reset. Redirecting to login...');
        setTimeout(() => this._router.navigate(['/login']), 2000);
      },
      error: (err: any) => {
        this.isLoading.set(false);
        const status = err?.status;
        if (status === 400) {
          const backendMsg = typeof err?.error === 'string' ? err.error : err?.error?.message;
          this.errorMessage.set(backendMsg || 'Invalid or expired reset token.');
        } else {
          this.errorMessage.set('Something went wrong. Please try again later.');
        }
      },
    });
  }
}
