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
});
