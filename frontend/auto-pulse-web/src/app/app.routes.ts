import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './guards/auth.guard';

export const routes: Routes = [
  // Public routes
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent), canActivate: [guestGuard] },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent), canActivate: [guestGuard] },
  
  // Protected routes
  { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [authGuard] },
  { path: 'search', loadComponent: () => import('./pages/user-search/user-search.component').then(m => m.UserSearchComponent), canActivate: [authGuard] },
  { path: 'cars', loadComponent: () => import('./pages/cars-list/cars-list.component').then(m => m.CarsListComponent), canActivate: [authGuard] },
  
  // Legacy routes (для совместимости)
  { path: 'markets', loadComponent: () => import('./pages/markets-list/markets-list.component').then(m => m.MarketsListComponent), canActivate: [authGuard] },
  { path: 'dealers', loadComponent: () => import('./pages/dealers-list/dealers-list.component').then(m => m.DealersListComponent), canActivate: [authGuard] },
  { path: 'dealers/:id', loadComponent: () => import('./pages/dealer-details/dealer-details.component').then(m => m.DealerDetailsComponent), canActivate: [authGuard] },
  { path: 'markets/:id', loadComponent: () => import('./pages/market-details/market-details.component').then(m => m.MarketDetailsComponent), canActivate: [authGuard] },
  
  // 404
  { path: '**', redirectTo: 'dashboard' }
];
