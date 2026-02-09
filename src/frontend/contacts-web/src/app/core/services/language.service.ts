import { inject, Injectable, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

export type SupportedLanguage = 'tr' | 'en';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly translateService = inject(TranslateService);
  private readonly storageKey = 'contacts.language';

  readonly currentLanguage = signal<SupportedLanguage>('tr');

  initialize(): void {
    this.translateService.setFallbackLang('en');

    const stored = localStorage.getItem(this.storageKey);
    const language: SupportedLanguage = stored === 'en' ? 'en' : 'tr';

    this.applyLanguage(language, false);
  }

  setLanguage(language: SupportedLanguage): void {
    this.applyLanguage(language, true);
  }

  private applyLanguage(language: SupportedLanguage, persist: boolean): void {
    this.translateService.use(language);
    this.currentLanguage.set(language);

    if (persist) {
      localStorage.setItem(this.storageKey, language);
    }
  }
}
