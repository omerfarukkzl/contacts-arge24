import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AppShellComponent } from './app-shell.component';

describe('AppShellComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppShellComponent, TranslateModule.forRoot()],
      providers: [provideRouter([]), provideHttpClient()]
    }).compileComponents();
  });

  afterEach(() => {
    localStorage.removeItem('contacts.theme');
    document.documentElement.removeAttribute('data-theme');
    document.documentElement.style.removeProperty('color-scheme');
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(AppShellComponent);
    const component = fixture.componentInstance;
    expect(component).toBeTruthy();
  });

  it('should open and close mobile navigation', () => {
    const fixture = TestBed.createComponent(AppShellComponent);
    const component = fixture.componentInstance;

    expect(component.mobileNavOpen()).toBe(false);

    component.toggleMobileNav();
    expect(component.mobileNavOpen()).toBe(true);

    component.closeMobileNav();
    expect(component.mobileNavOpen()).toBe(false);
  });

  it('should toggle theme and update aria state', () => {
    const fixture = TestBed.createComponent(AppShellComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const themeToggleButton: HTMLButtonElement = fixture.nativeElement.querySelector('[data-testid="theme-toggle"]');

    expect(component.currentTheme()).toBe('light');
    expect(themeToggleButton.getAttribute('aria-pressed')).toBe('false');

    themeToggleButton.click();
    fixture.detectChanges();

    expect(component.currentTheme()).toBe('dark');
    expect(themeToggleButton.getAttribute('aria-pressed')).toBe('true');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });
});
