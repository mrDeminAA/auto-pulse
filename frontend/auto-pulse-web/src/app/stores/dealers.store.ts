import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap } from 'rxjs';
import { Dealer, PagedResult } from '../models/dealer.model';
import { DealersService } from '../services/dealers.service';

interface DealersState {
  dealers: Dealer[];
  pagination: {
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
  };
  loading: boolean;
  error: string | null;
}

const initialState: DealersState = {
  dealers: [],
  pagination: {
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0,
    hasPrevious: false,
    hasNext: false
  },
  loading: false,
  error: null
};

export interface LoadDealersParams {
  page?: number;
  pageSize?: number;
  marketId?: number;
  minRating?: number;
}

export const DealersStore = signalStore(
  { providedIn: 'root' },
  withState<DealersState>(initialState),
  withComputed((state) => ({
    dealersCount: computed(() => state.pagination().totalCount)
  })),
  withMethods((store, dealersService = inject(DealersService)) => ({
    loadDealers: rxMethod<LoadDealersParams>(
      pipe(
        tap((params) => {
          patchState(store, {
            loading: true,
            error: null,
            pagination: { ...store.pagination(), page: params.page || 1 }
          });
        }),
        switchMap((params) =>
          dealersService.getAllDealers(
            params.page,
            params.pageSize,
            params.marketId,
            params.minRating
          )
        ),
        tap({
          next: (result) =>
            patchState(store, {
              dealers: result.items,
              pagination: {
                totalCount: result.totalCount,
                page: result.page,
                pageSize: result.pageSize,
                totalPages: result.totalPages,
                hasPrevious: result.hasPrevious,
                hasNext: result.hasNext
              },
              loading: false
            }),
          error: (error) => patchState(store, { error: error.message, loading: false })
        })
      )
    )
  }))
);
