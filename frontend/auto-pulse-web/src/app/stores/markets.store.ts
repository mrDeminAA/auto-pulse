import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap } from 'rxjs';
import { Market } from '../models/market.model';
import { MarketsService } from '../services/markets.service';

interface MarketsState {
  markets: Market[];
  loading: boolean;
  error: string | null;
}

const initialState: MarketsState = {
  markets: [],
  loading: false,
  error: null
};

export const MarketsStore = signalStore(
  { providedIn: 'root' },
  withState<MarketsState>(initialState),
  withComputed((state) => ({
    marketsCount: computed(() => state.markets().length)
  })),
  withMethods((store, marketsService = inject(MarketsService)) => ({
    loadMarkets: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap(() => marketsService.getAllMarkets()),
        tap({
          next: (markets) => patchState(store, { markets, loading: false }),
          error: (error) => patchState(store, { error: error.message, loading: false })
        })
      )
    )
  }))
);
