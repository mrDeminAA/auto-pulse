import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  UserDto,
  UpdateProfileRequest
} from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/auth';
  private readonly tokenKey = 'auth_token';
  private readonly userKey = 'auth_user';

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => this.setToken(response.token, response.expiresAt)),
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Очищаем токен при ошибке авторизации
          this.logout();
          return throwError(() => new Error('Неверный email или пароль'));
        }
        return throwError(() => new Error(error.message || 'Ошибка при входе'));
      })
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => this.setToken(response.token, response.expiresAt)),
      catchError((error: HttpErrorResponse) => {
        if (error.status === 400) {
          const errorMessage = error.error?.message || 'Ошибка при регистрации';
          return throwError(() => new Error(errorMessage));
        }
        return throwError(() => new Error(error.message || 'Ошибка при регистрации'));
      })
    );
  }

  getCurrentUser(): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/me`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.apiUrl}/me`, request);
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    const expiresAt = localStorage.getItem(`${this.tokenKey}_expires`);
    if (!expiresAt) return false;

    return new Date(expiresAt) > new Date();
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUser(): UserDto | null {
    const userStr = localStorage.getItem(this.userKey);
    return userStr ? JSON.parse(userStr) : null;
  }

  setToken(token: string, expiresAt: string): void {
    localStorage.setItem(this.tokenKey, token);
    localStorage.setItem(`${this.tokenKey}_expires`, expiresAt);
  }

  setUser(user: UserDto): void {
    localStorage.setItem(this.userKey, JSON.stringify(user));
  }
}
