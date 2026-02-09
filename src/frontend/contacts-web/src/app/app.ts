import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageService, SupportedLanguage } from './core/services/language.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  private readonly languageService = inject(LanguageService);

  readonly currentLanguage = this.languageService.currentLanguage;
  readonly supportedLanguages: SupportedLanguage[] = ['tr', 'en'];

  switchLanguage(language: SupportedLanguage): void {
    this.languageService.setLanguage(language);
  }
}
