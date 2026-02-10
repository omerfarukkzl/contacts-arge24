import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPageComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly authApiService = inject(AuthApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly loginForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  async submit(): Promise<void> {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const formValue = this.loginForm.getRawValue();
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      await this.authService.login(formValue.email, formValue.password);
      await this.completeSuccessfulLogin();
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
      await this.completeSuccessfulLogin();
    } catch (error) {
      this.errorMessage.set(this.authService.mapErrorToTranslationKey(error));
    } finally {
      this.loading.set(false);
    }
  }

  private async completeSuccessfulLogin(): Promise<void> {
    await firstValueFrom(this.authApiService.getMe());
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/contacts';
    await this.router.navigateByUrl(returnUrl);
  }
}
