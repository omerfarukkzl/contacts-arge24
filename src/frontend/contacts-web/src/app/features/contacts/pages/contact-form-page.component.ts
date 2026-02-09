import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { Contact, UpsertContactRequest } from '../../../core/models/contact.model';
import { ContactsApiService } from '../../../core/services/contacts-api.service';

@Component({
  selector: 'app-contact-form-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './contact-form-page.component.html',
  styleUrl: './contact-form-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactFormPageComponent {
  private readonly contactsApiService = inject(ContactsApiService);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  readonly contactId = signal<string | null>(this.activatedRoute.snapshot.paramMap.get('id'));
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly isEditMode = computed(() => this.contactId() !== null);

  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    phone: ['', [Validators.required, Validators.maxLength(30)]],
    email: ['', [Validators.email, Validators.maxLength(200)]],
    company: ['', [Validators.maxLength(200)]],
    notes: [''],
    isFavorite: [false],
    tagsText: ['']
  });

  constructor() {
    const id = this.contactId();
    if (id) {
      void this.loadContact(id);
    }
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const payload = this.mapFormToRequest();

    try {
      const id = this.contactId();

      if (id) {
        const updatedContact = await firstValueFrom(this.contactsApiService.updateContact(id, payload));
        await this.router.navigate(['/contacts', updatedContact.id]);
      } else {
        const createdContact = await firstValueFrom(this.contactsApiService.createContact(payload));
        await this.router.navigate(['/contacts', createdContact.id]);
      }
    } catch (error: unknown) {
      this.errorMessage.set(this.resolveSubmitErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  private async loadContact(id: string): Promise<void> {
    this.loading.set(true);

    try {
      const contact = await firstValueFrom(this.contactsApiService.getContact(id));
      this.patchForm(contact);
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }

  private patchForm(contact: Contact): void {
    this.form.patchValue({
      firstName: contact.firstName,
      lastName: contact.lastName,
      phone: contact.phone,
      email: contact.email ?? '',
      company: contact.company ?? '',
      notes: contact.notes ?? '',
      isFavorite: contact.isFavorite,
      tagsText: contact.tags.map((tag) => tag.name).join(', ')
    });
  }

  private mapFormToRequest(): UpsertContactRequest {
    const formValue = this.form.getRawValue();

    return {
      firstName: formValue.firstName.trim(),
      lastName: formValue.lastName.trim(),
      phone: formValue.phone.trim(),
      email: formValue.email.trim() || null,
      company: formValue.company.trim() || null,
      notes: formValue.notes.trim() || null,
      isFavorite: formValue.isFavorite,
      tags: formValue.tagsText
        .split(/[;,]/)
        .map((tag) => tag.trim())
        .filter((tag) => tag.length > 0)
    };
  }

  private resolveSubmitErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 409) {
      return 'ERRORS.DUPLICATE_PHONE';
    }

    return 'ERRORS.REQUEST_FAILED';
  }
}
