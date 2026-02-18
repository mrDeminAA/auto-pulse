import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MarketsStore } from '../../stores/markets.store';
import { Market } from '../../models/market.model';

@Component({
  selector: 'app-markets-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './markets-list.component.html',
  styleUrls: ['./markets-list.component.scss']
})
export class MarketsListComponent implements OnInit {
  private readonly marketsStore = inject(MarketsStore);

  readonly markets = this.marketsStore.markets;
  readonly loading = this.marketsStore.loading;
  readonly error = this.marketsStore.error;
  readonly marketsCount = this.marketsStore.marketsCount;

  ngOnInit(): void {
    this.marketsStore.loadMarkets();
  }

  trackByMarketId(index: number, market: Market): number {
    return market.id;
  }
}
