import { APP_INITIALIZER, ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { routes } from './app.routes';
import { LanguageService } from './core/services/language.service';
import { ThemeService } from './core/services/theme.service';
import { AuthService } from './core/services/auth.service';
import { authTokenInterceptor } from './core/interceptors/auth-token.interceptor';
import { authErrorInterceptor } from './core/interceptors/auth-error.interceptor';

function initializeLanguage(languageService: LanguageService): () => void {
  return () => languageService.initialize();
}

function initializeTheme(themeService: ThemeService): () => void {
  return () => themeService.initialize();
}

function initializeAuth(authService: AuthService): () => Promise<void> {
  return () => authService.initialize();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([authTokenInterceptor, authErrorInterceptor])),
    provideRouter(routes),
    importProvidersFrom(
      TranslateModule.forRoot({
        fallbackLang: 'tr'
      })
    ),
    ...provideTranslateHttpLoader({
      prefix: '/i18n/',
      suffix: '.json',
      enforceLoading: true
    }),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeAuth,
      deps: [AuthService],
      multi: true
    },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeLanguage,
      deps: [LanguageService],
      multi: true
    },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeTheme,
      deps: [ThemeService],
      multi: true
    }
  ]
};
