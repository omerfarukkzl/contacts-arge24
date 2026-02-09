import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Tag } from '../models/tag.model';

@Injectable({ providedIn: 'root' })
export class TagsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/tags';

  getTags(): Observable<Tag[]> {
    return this.http.get<Tag[]>(this.baseUrl);
  }

  createTag(name: string): Observable<Tag> {
    return this.http.post<Tag>(this.baseUrl, { name });
  }

  deleteTag(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
