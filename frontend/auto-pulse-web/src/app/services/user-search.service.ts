import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  UserSearchRequest,
  UserSearchResponse,
  UserSearchStatus
} from '../models/user-search.model';

@Injectable({
  providedIn: 'root'
})
export class UserSearchService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/user/search';

  getUserSearch(): Observable<UserSearchResponse> {
    return this.http.get<UserSearchResponse>(this.apiUrl);
  }

  saveUserSearch(request: UserSearchRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(this.apiUrl, request);
  }

  deleteUserSearch(): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(this.apiUrl);
  }

  getSearchStatus(): Observable<UserSearchStatus> {
    return this.http.get<UserSearchStatus>(`${this.apiUrl}/status`);
  }
}
