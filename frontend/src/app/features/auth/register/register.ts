import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const pw = control.get('password')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return pw && confirm && pw !== confirm ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class RegisterComponent {
  private readonly _fb = inject(FormBuilder);
  private readonly _auth = inject(AuthService);
  private readonly _router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);
  readonly registrationSuccess = signal(false);

  readonly form = this._fb.group({
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(30), Validators.pattern(/^[a-zA-Z0-9_]+$/)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]],
  }, { validators: passwordMatchValidator });

  get usernameCtrl() { return this.form.get('username')!; }
  get emailCtrl()    { return this.form.get('email')!; }
  get passwordCtrl() { return this.form.get('password')!; }
  get confirmCtrl()  { return this.form.get('confirmPassword')!; }

  get passwordStrength(): number {
    const pw = this.passwordCtrl.value ?? '';
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw)) score++;
    if (/[0-9]/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    return score;
  }

  get strengthLabel(): string {
    return ['', 'Weak', 'Fair', 'Good', 'Strong'][this.passwordStrength];
  }

  get strengthClass(): string {
    return ['', 'strength-weak', 'strength-fair', 'strength-good', 'strength-strong'][this.passwordStrength];
  }

  togglePassword() { this.showPassword.update(v => !v); }

  submit() {
    if (this.form.invalid || this.isLoading()) return;
    this.errorMessage.set(null);
    this.isLoading.set(true);

    const { username, email, password } = this.form.value;

    this._auth.register(username!, email!, password!).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.registrationSuccess.set(true);
        // Auto-login after successful registration
        this._auth.login(email!, password!).subscribe({
          next: () => this._router.navigate(['/']),
          error: () => this._router.navigate(['/login']),
        });
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err?.status === 409) {
          this.errorMessage.set('An account with this email or username already exists.');
        } else if (err?.status === 400) {
          this.errorMessage.set('Please check your details and try again.');
        } else {
          this.errorMessage.set('Something went wrong. Please try again later.');
        }
      },
    });
  }
}
