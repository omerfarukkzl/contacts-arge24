import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import {
  Contact,
  ContactListQuery,
  ContactSortBy,
  SortDirection,
  UpsertContactRequest
} from '../../../core/models/contact.model';
import { Tag } from '../../../core/models/tag.model';
import { ContactsApiService } from '../../../core/services/contacts-api.service';
import { TagsApiService } from '../../../core/services/tags-api.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-contacts-list-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './contacts-list-page.component.html',
  styleUrl: './contacts-list-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactsListPageComponent implements OnDestroy {
  private readonly contactsApiService = inject(ContactsApiService);
  private readonly tagsApiService = inject(TagsApiService);
  private readonly formBuilder = inject(FormBuilder);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly undoVisible = signal(false);
  readonly contacts = signal<Contact[]>([]);
  readonly tags = signal<Tag[]>([]);
  readonly lastDeletedContactId = signal<string | null>(null);

  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  private undoTimer: ReturnType<typeof setTimeout> | null = null;

  readonly hasPreviousPage = computed(() => this.page() > 1);
  readonly hasNextPage = computed(() => this.page() < this.totalPages());

  readonly filterForm = this.formBuilder.nonNullable.group({
    search: [''],
    favoriteOnly: [false],
    tag: [''],
    company: [''],
    sortBy: this.formBuilder.nonNullable.control<ContactSortBy>('firstName'),
    sortDir: this.formBuilder.nonNullable.control<SortDirection>('asc')
  });

  constructor() {
    void this.loadTags();
    void this.loadContacts();
  }

  ngOnDestroy(): void {
    this.clearUndoTimer();
  }

  async applyFilters(): Promise<void> {
    this.page.set(1);
    await this.loadContacts();
  }

  async resetFilters(): Promise<void> {
    this.filterForm.reset({
      search: '',
      favoriteOnly: false,
      tag: '',
      company: '',
      sortBy: 'firstName',
      sortDir: 'asc'
    });
    this.page.set(1);
    await this.loadContacts();
  }

  async goToPreviousPage(): Promise<void> {
    if (!this.hasPreviousPage()) {
      return;
    }

    this.page.update((currentPage) => currentPage - 1);
    await this.loadContacts();
  }

  async goToNextPage(): Promise<void> {
    if (!this.hasNextPage()) {
      return;
    }

    this.page.update((currentPage) => currentPage + 1);
    await this.loadContacts();
  }

  async toggleFavorite(contact: Contact): Promise<void> {
    const payload: UpsertContactRequest = {
      firstName: contact.firstName,
      lastName: contact.lastName,
      phone: contact.phone,
      email: contact.email,
      company: contact.company,
      notes: contact.notes,
      isFavorite: !contact.isFavorite,
      tags: contact.tags.map((tag) => tag.name)
    };

    this.loading.set(true);
    try {
      await firstValueFrom(this.contactsApiService.updateContact(contact.id, payload));
      await this.loadContacts();
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }

  async deleteContact(contact: Contact): Promise<void> {
    const accepted = window.confirm('Delete this contact?');
    if (!accepted) {
      return;
    }

    this.loading.set(true);
    try {
      await firstValueFrom(this.contactsApiService.deleteContact(contact.id));
      this.lastDeletedContactId.set(contact.id);
      this.showUndoForFifteenSeconds();
      await this.loadContacts();
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }

  async undoDelete(): Promise<void> {
    const deletedContactId = this.lastDeletedContactId();
    if (!deletedContactId) {
      return;
    }

    this.loading.set(true);
    try {
      await firstValueFrom(this.contactsApiService.restoreContact(deletedContactId));
      this.hideUndo();
      await this.loadContacts();
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }

  formatTags(tags: Tag[]): string {
    return tags.map((tag) => tag.name).join(', ');
  }

  private async loadTags(): Promise<void> {
    try {
      const fetchedTags = await firstValueFrom(this.tagsApiService.getTags());
      this.tags.set(fetchedTags);
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    }
  }

  private async loadContacts(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);

    const formValue = this.filterForm.getRawValue();
    const query: ContactListQuery = {
      search: formValue.search || undefined,
      page: this.page(),
      pageSize: this.pageSize(),
      sortBy: formValue.sortBy,
      sortDir: formValue.sortDir,
      favoriteOnly: formValue.favoriteOnly,
      tag: formValue.tag || undefined,
      company: formValue.company || undefined
    };

    try {
      const response = await firstValueFrom(this.contactsApiService.getContacts(query));
      this.contacts.set(response.items);
      this.totalCount.set(response.totalCount);
      this.totalPages.set(response.totalPages);
      this.page.set(response.page);
      this.pageSize.set(response.pageSize);
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }

  private showUndoForFifteenSeconds(): void {
    this.undoVisible.set(true);
    this.clearUndoTimer();
    this.undoTimer = setTimeout(() => {
      this.hideUndo();
    }, 15000);
  }

  private hideUndo(): void {
    this.undoVisible.set(false);
    this.lastDeletedContactId.set(null);
    this.clearUndoTimer();
  }

  private clearUndoTimer(): void {
    if (!this.undoTimer) {
      return;
    }

    clearTimeout(this.undoTimer);
    this.undoTimer = null;
  }
}
