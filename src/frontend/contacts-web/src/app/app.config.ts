import { APP_INITIALIZER, ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { routes } from './app.routes';
import { LanguageService } from './core/services/language.service';

function initializeLanguage(languageService: LanguageService): () => void {
  return () => languageService.initialize();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    provideRouter(routes),
    importProvidersFrom(
      TranslateModule.forRoot({
        fallbackLang: 'tr'
      })
    ),
    ...provideTranslateHttpLoader({
      prefix: '/i18n/',
      suffix: '.json'
    }),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeLanguage,
      deps: [LanguageService],
      multi: true
    }
  ]
};
