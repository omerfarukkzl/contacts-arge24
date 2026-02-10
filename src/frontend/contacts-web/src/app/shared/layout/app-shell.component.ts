import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageService, SupportedLanguage } from '../../core/services/language.service';
import { ThemeService } from '../../core/services/theme.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppShellComponent {
  private readonly languageService = inject(LanguageService);
  private readonly themeService = inject(ThemeService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentLanguage = this.languageService.currentLanguage;
  readonly currentTheme = this.themeService.currentTheme;
  readonly userEmail = this.authService.userEmail;
  readonly supportedLanguages: SupportedLanguage[] = ['tr', 'en'];
  readonly mobileNavOpen = signal(false);

  switchLanguage(language: SupportedLanguage): void {
    this.languageService.setLanguage(language);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  toggleMobileNav(): void {
    this.mobileNavOpen.update((currentValue) => !currentValue);
  }

  closeMobileNav(): void {
    this.mobileNavOpen.set(false);
  }

  async logout(): Promise<void> {
    await this.authService.logout();
    this.closeMobileNav();
    await this.router.navigate(['/auth/login']);
  }
}
