import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MarketsService } from '../../services/markets.service';
import { MarketDetails } from '../../models/market.model';

@Component({
  selector: 'app-market-details',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './market-details.component.html',
  styleUrls: ['./market-details.component.scss']
})
export class MarketDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly marketsService = inject(MarketsService);

  market: MarketDetails | null = null;
  loading = false;
  error: string | null = null;

  ngOnInit(): void {
    const marketId = this.route.snapshot.paramMap.get('id');
    if (marketId) {
      this.loadMarket(+marketId);
    } else {
      this.error = 'ID рынка не указан';
    }
  }

  loadMarket(id: number): void {
    this.loading = true;
    this.marketsService.getMarketById(id).subscribe({
      next: (data) => {
        this.market = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Ошибка загрузки данных о рынке';
        this.loading = false;
        console.error(err);
      }
    });
  }
}
