import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss'],
})
export class ForgotPasswordComponent {
  private readonly _fb = inject(FormBuilder);
  private readonly _auth = inject(AuthService);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly loadingMessage = signal<string>('Sending...');

  readonly form = this._fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  get emailCtrl() { return this.form.get('email')!; }

  submit() {
    if (this.form.invalid || this.isLoading()) return;
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.isLoading.set(true);

    const { email } = this.form.value;

    this._auth.forgotPassword(email!).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.successMessage.set('If the email is registered, a password reset link has been sent.');
        this.form.reset();
      },
      error: (err: any) => {
        this.isLoading.set(false);
        this.errorMessage.set('Something went wrong. Please try again later.');
      },
    });
  }
}
