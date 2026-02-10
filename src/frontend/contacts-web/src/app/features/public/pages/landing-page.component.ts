import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageService, SupportedLanguage } from '../../../core/services/language.service';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingPageComponent {
  private readonly languageService = inject(LanguageService);

  readonly currentLanguage = this.languageService.currentLanguage;
  readonly supportedLanguages: SupportedLanguage[] = ['tr', 'en'];

  switchLanguage(language: SupportedLanguage): void {
    this.languageService.setLanguage(language);
  }
}
