import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Contact, ContactListQuery, UpsertContactRequest } from '../models/contact.model';
import { PagedResult } from '../models/paged-result.model';
import { CsvImportResult } from '../models/csv-import-result.model';

@Injectable({ providedIn: 'root' })
export class ContactsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/contacts';

  getContacts(query: ContactListQuery): Observable<PagedResult<Contact>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize))
      .set('sortBy', query.sortBy)
      .set('sortDir', query.sortDir)
      .set('favoriteOnly', String(query.favoriteOnly));

    if (query.search) {
      params = params.set('search', query.search);
    }

    if (query.tag) {
      params = params.set('tag', query.tag);
    }

    if (query.company) {
      params = params.set('company', query.company);
    }

    return this.http.get<PagedResult<Contact>>(this.baseUrl, { params });
  }

  getContact(id: string): Observable<Contact> {
    return this.http.get<Contact>(`${this.baseUrl}/${id}`);
  }

  createContact(payload: UpsertContactRequest): Observable<Contact> {
    return this.http.post<Contact>(this.baseUrl, payload);
  }

  updateContact(id: string, payload: UpsertContactRequest): Observable<Contact> {
    return this.http.put<Contact>(`${this.baseUrl}/${id}`, payload);
  }

  deleteContact(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  restoreContact(id: string): Observable<Contact> {
    return this.http.post<Contact>(`${this.baseUrl}/${id}/restore`, {});
  }

  exportContacts(query: Pick<ContactListQuery, 'search' | 'tag'>): Observable<Blob> {
    let params = new HttpParams();

    if (query.search) {
      params = params.set('search', query.search);
    }

    if (query.tag) {
      params = params.set('tag', query.tag);
    }

    return this.http.get(`${this.baseUrl}/export`, {
      params,
      responseType: 'blob'
    });
  }

  importContacts(file: File): Observable<CsvImportResult> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<CsvImportResult>(`${this.baseUrl}/import`, formData);
  }
}
