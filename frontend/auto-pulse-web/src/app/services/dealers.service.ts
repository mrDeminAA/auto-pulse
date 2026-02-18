import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Dealer, DealerDetails, CarBrief, PagedResult, DealerStats } from '../models/dealer.model';

@Injectable({
  providedIn: 'root'
})
export class DealersService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5001/api/dealers';

  getAllDealers(
    page: number = 1,
    pageSize: number = 20,
    marketId?: number,
    minRating?: number
  ): Observable<PagedResult<Dealer>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (marketId) params = params.set('marketId', marketId.toString());
    if (minRating) params = params.set('minRating', minRating.toString());

    return this.http.get<PagedResult<Dealer>>(this.apiUrl, { params });
  }

  getDealerById(id: number): Observable<DealerDetails | null> {
    return this.http.get<DealerDetails>(`${this.apiUrl}/${id}`);
  }

  getDealersByMarket(marketId: number): Observable<Dealer[]> {
    return this.http.get<Dealer[]>(`${this.apiUrl}/market/${marketId}`);
  }

  getDealerCars(
    dealerId: number,
    page: number = 1,
    pageSize: number = 20
  ): Observable<PagedResult<CarBrief> | null> {
    return this.http.get<PagedResult<CarBrief>>(`${this.apiUrl}/${dealerId}/cars`, {
      params: new HttpParams()
        .set('page', page.toString())
        .set('pageSize', pageSize.toString())
    });
  }

  getDealerStats(): Observable<DealerStats> {
    return this.http.get<DealerStats>(`${this.apiUrl}/stats/summary`);
  }
}
