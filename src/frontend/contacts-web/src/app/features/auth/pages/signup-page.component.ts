import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-signup-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './signup-page.component.html',
  styleUrl: './signup-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SignupPageComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly authApiService = inject(AuthApiService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly signupForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  async submit(): Promise<void> {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }

    const formValue = this.signupForm.getRawValue();
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      await this.authService.signup(formValue.email, formValue.password);
      await this.completeSuccessfulSignup();
    } catch (error) {
      this.errorMessage.set(this.authService.mapErrorToTranslationKey(error));
    } finally {
      this.loading.set(false);
    }
  }

  async submitWithGoogle(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      await this.authService.loginWithGoogle();
      await this.completeSuccessfulSignup();
    } catch (error) {
      this.errorMessage.set(this.authService.mapErrorToTranslationKey(error));
    } finally {
      this.loading.set(false);
    }
  }

  private async completeSuccessfulSignup(): Promise<void> {
    await firstValueFrom(this.authApiService.getMe());
    await this.router.navigateByUrl('/contacts');
  }
}
