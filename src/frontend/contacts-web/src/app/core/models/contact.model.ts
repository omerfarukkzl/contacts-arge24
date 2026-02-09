import { Tag } from './tag.model';

export type ContactSortBy = 'firstName' | 'lastName' | 'createdAt';
export type SortDirection = 'asc' | 'desc';

export interface Contact {
  id: string;
  firstName: string;
  lastName: string;
  phone: string;
  email?: string | null;
  company?: string | null;
  notes?: string | null;
  isFavorite: boolean;
  createdAt: string;
  updatedAt?: string | null;
  tags: Tag[];
}

export interface ContactListQuery {
  search?: string;
  page: number;
  pageSize: number;
  sortBy: ContactSortBy;
  sortDir: SortDirection;
  favoriteOnly: boolean;
  tag?: string;
  company?: string;
}

export interface UpsertContactRequest {
  firstName: string;
  lastName: string;
  phone: string;
  email?: string | null;
  company?: string | null;
  notes?: string | null;
  isFavorite: boolean;
  tags: string[];
}
