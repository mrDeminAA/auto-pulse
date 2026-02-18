import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Market, MarketDetails, DealerBrief, CarsCount } from '../models/market.model';

@Injectable({
  providedIn: 'root'
})
export class MarketsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5001/api/markets';

  getAllMarkets(): Observable<Market[]> {
    return this.http.get<Market[]>(this.apiUrl);
  }

  getMarketById(id: number): Observable<MarketDetails | null> {
    return this.http.get<MarketDetails>(`${this.apiUrl}/${id}`);
  }

  getMarketDealers(marketId: number): Observable<DealerBrief[]> {
    return this.http.get<DealerBrief[]>(`${this.apiUrl}/${marketId}/dealers`);
  }

  getMarketCarsCount(marketId: number): Observable<CarsCount | null> {
    return this.http.get<CarsCount>(`${this.apiUrl}/${marketId}/cars/count`);
  }
}
