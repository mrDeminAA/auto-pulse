import { Routes } from '@angular/router';
import { MarketsListComponent } from './pages/markets-list/markets-list.component';
import { DealersListComponent } from './pages/dealers-list/dealers-list.component';
import { DealerDetailsComponent } from './pages/dealer-details/dealer-details.component';
import { MarketDetailsComponent } from './pages/market-details/market-details.component';
import { CarsListComponent } from './pages/cars-list/cars-list.component';

export const routes: Routes = [
  { path: '', redirectTo: 'markets', pathMatch: 'full' },
  { path: 'markets', component: MarketsListComponent },
  { path: 'dealers', component: DealersListComponent },
  { path: 'cars', component: CarsListComponent },
  { path: 'dealers/:id', component: DealerDetailsComponent },
  { path: 'markets/:id', component: MarketDetailsComponent }
];
