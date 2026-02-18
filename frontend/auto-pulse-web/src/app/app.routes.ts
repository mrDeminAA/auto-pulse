import { Routes } from '@angular/router';
import { MarketsListComponent } from './pages/markets-list/markets-list.component';
import { DealersListComponent } from './pages/dealers-list/dealers-list.component';

export const routes: Routes = [
  { path: '', redirectTo: 'markets', pathMatch: 'full' },
  { path: 'markets', component: MarketsListComponent },
  { path: 'dealers', component: DealersListComponent }
];
