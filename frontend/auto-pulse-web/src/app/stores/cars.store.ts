import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap } from 'rxjs';
import { CarDto, PagedResult } from '../models/dealer.model';
import { CarsService } from '../services/cars.service';

interface CarsState {
  cars: CarDto[];
  loading: boolean;
  error: string | null;
  totalCount: number;
}

const initialState: CarsState = {
  cars: [],
  loading: false,
  error: null,
  totalCount: 0
};

export const CarsStore = signalStore(
  { providedIn: 'root' },
  withState<CarsState>(initialState),
  withMethods((store, carsService = inject(CarsService)) => ({
    loadCars: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap(() => carsService.getAllCars(1, 50)),
        tap({
          next: (result: PagedResult<CarDto>) => patchState(store, {
            cars: result.items,
            totalCount: result.totalCount,
            loading: false
          }),
          error: (error: Error) => patchState(store, { error: error.message, loading: false })
        })
      )
    )
  }))
);
