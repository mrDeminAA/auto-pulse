import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CarDto, PagedResult } from '../models/dealer.model';

@Injectable({
  providedIn: 'root'
})
export class CarsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5001/api/cars';

  getAllCars(page: number = 1, pageSize: number = 20): Observable<PagedResult<CarDto>> {
    return this.http.get<PagedResult<CarDto>>(this.apiUrl, {
      params: { page, pageSize }
    });
  }

  getCarById(id: number): Observable<CarDto | null> {
    return this.http.get<CarDto>(`${this.apiUrl}/${id}`);
  }
}
