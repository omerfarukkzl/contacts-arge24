import { Routes } from '@angular/router';
import { AppShellComponent } from './shared/layout/app-shell.component';
import { authGuard } from './core/guards/auth.guard';
import { redirectAuthenticatedGuard } from './core/guards/redirect-authenticated.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canActivate: [redirectAuthenticatedGuard],
    loadComponent: () => import('./features/public/pages/landing-page.component').then((module) => module.LandingPageComponent)
  },
  {
    path: 'auth/login',
    canActivate: [redirectAuthenticatedGuard],
    loadComponent: () => import('./features/auth/pages/login-page.component').then((module) => module.LoginPageComponent)
  },
  {
    path: 'auth/signup',
    canActivate: [redirectAuthenticatedGuard],
    loadComponent: () => import('./features/auth/pages/signup-page.component').then((module) => module.SignupPageComponent)
  },
  {
    path: 'auth/forgot-password',
    canActivate: [redirectAuthenticatedGuard],
    loadComponent: () =>
      import('./features/auth/pages/forgot-password-page.component').then((module) => module.ForgotPasswordPageComponent)
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'contacts',
        loadComponent: () =>
          import('./features/contacts/pages/contacts-list-page.component').then((module) => module.ContactsListPageComponent)
      },
      {
        path: 'contacts/new',
        loadComponent: () =>
          import('./features/contacts/pages/contact-form-page.component').then((module) => module.ContactFormPageComponent)
      },
      {
        path: 'contacts/import-export',
        loadComponent: () =>
          import('./features/contacts/pages/import-export-page.component').then((module) => module.ImportExportPageComponent)
      },
      {
        path: 'contacts/:id/edit',
        loadComponent: () =>
          import('./features/contacts/pages/contact-form-page.component').then((module) => module.ContactFormPageComponent)
      },
      {
        path: 'contacts/:id',
        loadComponent: () =>
          import('./features/contacts/pages/contact-detail-page.component').then((module) => module.ContactDetailPageComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
