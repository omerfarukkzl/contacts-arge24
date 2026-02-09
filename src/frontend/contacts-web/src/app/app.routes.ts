import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'contacts'
  },
  {
    path: 'contacts',
    loadComponent: () =>
      import('./features/contacts/pages/contacts-list-page.component').then(
        (module) => module.ContactsListPageComponent
      )
  },
  {
    path: 'contacts/new',
    loadComponent: () =>
      import('./features/contacts/pages/contact-form-page.component').then(
        (module) => module.ContactFormPageComponent
      )
  },
  {
    path: 'contacts/import-export',
    loadComponent: () =>
      import('./features/contacts/pages/import-export-page.component').then(
        (module) => module.ImportExportPageComponent
      )
  },
  {
    path: 'contacts/:id/edit',
    loadComponent: () =>
      import('./features/contacts/pages/contact-form-page.component').then(
        (module) => module.ContactFormPageComponent
      )
  },
  {
    path: 'contacts/:id',
    loadComponent: () =>
      import('./features/contacts/pages/contact-detail-page.component').then(
        (module) => module.ContactDetailPageComponent
      )
  },
  {
    path: '**',
    redirectTo: 'contacts'
  }
];
