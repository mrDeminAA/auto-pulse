import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap, catchError, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { UserDto, LoginRequest, RegisterRequest, UpdateProfileRequest } from '../models/auth.model';

interface AuthState {
  user: UserDto | null;
  token: string | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  loading: false,
  error: null
};

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState<AuthState>(initialState),
  withComputed((store) => ({
    userName: computed(() => store.user()?.name || store.user()?.email || 'Гость'),
    userEmail: computed(() => store.user()?.email || '')
  })),
  withMethods((store, authService = inject(AuthService)) => ({
    login: rxMethod<LoginRequest>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((request) => authService.login(request).pipe(
          tap((response) => {
            // После логина загружаем профиль
            authService.getCurrentUser().subscribe({
              next: (user) => {
                patchState(store, {
                  user,
                  token: response.token,
                  isAuthenticated: true,
                  loading: false
                });
                authService.setUser(user);
              },
              error: (error) => patchState(store, { error: error.message, loading: false })
            });
          }),
          catchError((error) => {
            patchState(store, { error: error.message, loading: false });
            return of(null);
          })
        ))
      )
    ),

    register: rxMethod<RegisterRequest>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((request) => authService.register(request).pipe(
          tap((response) => {
            authService.getCurrentUser().subscribe({
              next: (user) => {
                patchState(store, {
                  user,
                  token: response.token,
                  isAuthenticated: true,
                  loading: false
                });
                authService.setUser(user);
              },
              error: (error) => patchState(store, { error: error.message, loading: false })
            });
          }),
          catchError((error) => {
            patchState(store, { error: error.message, loading: false });
            return of(null);
          })
        ))
      )
    ),

    loadUser: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { loading: true })),
        switchMap(() => authService.getCurrentUser().pipe(
          tap((user) => {
            patchState(store, {
              user,
              token: authService.getToken(),
              isAuthenticated: true,
              loading: false
            });
            authService.setUser(user);
          }),
          catchError(() => {
            patchState(store, { isAuthenticated: false, loading: false });
            return of(null);
          })
        ))
      )
    ),

    updateProfile: rxMethod<UpdateProfileRequest>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((request) => authService.updateProfile(request).pipe(
          tap((user) => {
            patchState(store, { user, loading: false });
            authService.setUser(user);
          }),
          catchError((error) => {
            patchState(store, { error: error.message, loading: false });
            return of(null);
          })
        ))
      )
    ),

    logout() {
      authService.logout();
      patchState(store, {
        user: null,
        token: null,
        isAuthenticated: false
      });
    },

    clearError() {
      patchState(store, { error: null });
    }
  }))
);
