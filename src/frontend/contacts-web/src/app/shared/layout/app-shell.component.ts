import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageService, SupportedLanguage } from '../../core/services/language.service';

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

  readonly currentLanguage = this.languageService.currentLanguage;
  readonly supportedLanguages: SupportedLanguage[] = ['tr', 'en'];
  readonly mobileNavOpen = signal(false);

  switchLanguage(language: SupportedLanguage): void {
    this.languageService.setLanguage(language);
  }

  toggleMobileNav(): void {
    this.mobileNavOpen.update((currentValue) => !currentValue);
  }

  closeMobileNav(): void {
    this.mobileNavOpen.set(false);
  }
}
