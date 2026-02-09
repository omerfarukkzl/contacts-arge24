import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';

function setMatchMedia(matches: boolean): void {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    configurable: true,
    value: (query: string): MediaQueryList =>
      ({
        matches,
        media: query,
        onchange: null,
        addListener: () => undefined,
        removeListener: () => undefined,
        addEventListener: () => undefined,
        removeEventListener: () => undefined,
        dispatchEvent: () => false
      }) as MediaQueryList
  });
}

describe('ThemeService', () => {
  let service: ThemeService;
  let originalMatchMedia: typeof window.matchMedia;

  beforeEach(() => {
    originalMatchMedia = window.matchMedia;
    localStorage.removeItem('contacts.theme');
    document.documentElement.removeAttribute('data-theme');
    document.documentElement.style.removeProperty('color-scheme');

    TestBed.configureTestingModule({});
    service = TestBed.inject(ThemeService);
  });

  afterEach(() => {
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      configurable: true,
      value: originalMatchMedia
    });

    localStorage.removeItem('contacts.theme');
    document.documentElement.removeAttribute('data-theme');
    document.documentElement.style.removeProperty('color-scheme');
  });

  it('should initialize from stored theme', () => {
    localStorage.setItem('contacts.theme', 'dark');

    service.initialize();

    expect(service.currentTheme()).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });

  it('should use system dark theme when no stored theme exists', () => {
    setMatchMedia(true);

    service.initialize();

    expect(service.currentTheme()).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });

  it('should use light theme when no stored theme exists and system is not dark', () => {
    setMatchMedia(false);

    service.initialize();

    expect(service.currentTheme()).toBe('light');
    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
  });

  it('should persist and apply selected theme', () => {
    service.setTheme('dark');

    expect(service.currentTheme()).toBe('dark');
    expect(localStorage.getItem('contacts.theme')).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(document.documentElement.style.getPropertyValue('color-scheme')).toBe('dark');
  });

  it('should toggle between themes', () => {
    service.setTheme('light');

    service.toggleTheme();
    expect(service.currentTheme()).toBe('dark');

    service.toggleTheme();
    expect(service.currentTheme()).toBe('light');
  });
});
