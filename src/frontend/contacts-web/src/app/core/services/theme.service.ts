import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';

export type SupportedTheme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly storageKey = 'contacts.theme';

  readonly currentTheme = signal<SupportedTheme>('light');

  initialize(): void {
    const theme = this.resolveInitialTheme();
    this.applyTheme(theme, false);
  }

  setTheme(theme: SupportedTheme): void {
    this.applyTheme(theme, true);
  }

  toggleTheme(): void {
    this.setTheme(this.currentTheme() === 'dark' ? 'light' : 'dark');
  }

  private resolveInitialTheme(): SupportedTheme {
    const storedTheme = this.readStoredTheme();
    if (storedTheme !== null) {
      return storedTheme;
    }

    return this.prefersDarkTheme() ? 'dark' : 'light';
  }

  private readStoredTheme(): SupportedTheme | null {
    const stored = this.document.defaultView?.localStorage.getItem(this.storageKey);
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }

    return null;
  }

  private prefersDarkTheme(): boolean {
    return this.document.defaultView?.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false;
  }

  private applyTheme(theme: SupportedTheme, persist: boolean): void {
    this.currentTheme.set(theme);
    this.document.documentElement.setAttribute('data-theme', theme);
    this.document.documentElement.style.setProperty('color-scheme', theme);

    if (persist) {
      this.document.defaultView?.localStorage.setItem(this.storageKey, theme);
    }
  }
}
