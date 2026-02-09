import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { Contact, UpsertContactRequest } from '../../../core/models/contact.model';
import { ContactsApiService } from '../../../core/services/contacts-api.service';
import { Tag } from '../../../core/models/tag.model';

@Component({
  selector: 'app-contact-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './contact-detail-page.component.html',
  styleUrl: './contact-detail-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactDetailPageComponent {
  private readonly contactsApiService = inject(ContactsApiService);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly translateService = inject(TranslateService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly contact = signal<Contact | null>(null);

  constructor() {
    const id = this.activatedRoute.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
      return;
    }

    void this.loadContact(id);
  }

  async toggleFavorite(): Promise<void> {
    const currentContact = this.contact();
    if (!currentContact) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const payload: UpsertContactRequest = {
      firstName: currentContact.firstName,
      lastName: currentContact.lastName,
      phone: currentContact.phone,
      email: currentContact.email,
      company: currentContact.company,
      notes: currentContact.notes,
      isFavorite: !currentContact.isFavorite,
      tags: currentContact.tags.map((tag) => tag.name)
    };

    try {
      const updatedContact = await firstValueFrom(
        this.contactsApiService.updateContact(currentContact.id, payload)
      );
      this.contact.set(updatedContact);
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }

  async deleteContact(): Promise<void> {
    const currentContact = this.contact();
    if (!currentContact) {
      return;
    }

    const accepted = window.confirm(this.translateService.instant('CONTACTS.CONFIRM_DELETE'));
    if (!accepted) {
      return;
    }

    this.loading.set(true);

    try {
      await firstValueFrom(this.contactsApiService.deleteContact(currentContact.id));
      await this.router.navigate(['/contacts']);
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }

  formatTags(tags: Tag[]): string {
    return tags.map((tag) => tag.name).join(', ');
  }

  getInitials(contact: Contact): string {
    return `${contact.firstName.charAt(0)}${contact.lastName.charAt(0)}`.toUpperCase();
  }

  private async loadContact(id: string): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const fetchedContact = await firstValueFrom(this.contactsApiService.getContact(id));
      this.contact.set(fetchedContact);
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }
}
