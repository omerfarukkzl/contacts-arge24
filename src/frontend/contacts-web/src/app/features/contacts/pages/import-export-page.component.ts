import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { CsvImportResult } from '../../../core/models/csv-import-result.model';
import { ContactsApiService } from '../../../core/services/contacts-api.service';

@Component({
  selector: 'app-import-export-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './import-export-page.component.html',
  styleUrl: './import-export-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImportExportPageComponent {
  private readonly contactsApiService = inject(ContactsApiService);
  private readonly formBuilder = inject(FormBuilder);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly infoMessage = signal<string | null>(null);
  readonly report = signal<CsvImportResult | null>(null);
  readonly selectedFile = signal<File | null>(null);

  readonly exportForm = this.formBuilder.nonNullable.group({
    search: [''],
    tag: ['']
  });

  async exportCsv(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);

    const formValue = this.exportForm.getRawValue();

    try {
      const blob = await firstValueFrom(
        this.contactsApiService.exportContacts({
          search: formValue.search || undefined,
          tag: formValue.tag || undefined
        })
      );

      const hasDataRows = await this.hasCsvDataRows(blob);
      if (!hasDataRows) {
        this.infoMessage.set('IMPORT_EXPORT.NO_DATA_FOR_EXPORT');
        return;
      }

      this.downloadBlob(blob, `contacts-${new Date().toISOString()}.csv`);
      this.infoMessage.set('IMPORT_EXPORT.EXPORT_SUCCESS');
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  async importCsv(): Promise<void> {
    const file = this.selectedFile();
    if (!file) {
      this.errorMessage.set('IMPORT_EXPORT.FILE_REQUIRED');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const importResult = await firstValueFrom(this.contactsApiService.importContacts(file));
      this.report.set(importResult);
    } catch {
      this.errorMessage.set('ERRORS.REQUEST_FAILED');
    } finally {
      this.loading.set(false);
    }
  }

  private downloadBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();

    URL.revokeObjectURL(url);
  }

  private async hasCsvDataRows(blob: Blob): Promise<boolean> {
    const csvText = await blob.text();
    const nonEmptyLines = csvText
      .split(/\r?\n/)
      .map((line) => line.trim())
      .filter((line) => line.length > 0);

    return nonEmptyLines.length > 1;
  }
}
