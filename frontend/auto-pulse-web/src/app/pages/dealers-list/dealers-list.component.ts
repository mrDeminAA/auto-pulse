import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DealersStore } from '../../stores/dealers.store';
import { Dealer } from '../../models/dealer.model';

@Component({
  selector: 'app-dealers-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dealers-list.component.html',
  styleUrls: ['./dealers-list.component.scss']
})
export class DealersListComponent implements OnInit {
  private readonly dealersStore = inject(DealersStore);

  readonly dealers = this.dealersStore.dealers;
  readonly loading = this.dealersStore.loading;
  readonly error = this.dealersStore.error;
  readonly pagination = this.dealersStore.pagination;

  page = 1;
  pageSize = 20;
  minRating?: number;

  ngOnInit(): void {
    this.loadDealers();
  }

  loadDealers(): void {
    this.dealersStore.loadDealers({
      page: this.page,
      pageSize: this.pageSize,
      minRating: this.minRating
    });
  }

  onPageChange(newPage: number): void {
    this.page = newPage;
    this.loadDealers();
  }

  onFilterChange(): void {
    this.page = 1;
    this.loadDealers();
  }

  trackByDealerId(index: number, dealer: Dealer): number {
    return dealer.id;
  }

  getRatingClass(rating: number): string {
    if (rating >= 4.5) return 'text-success';
    if (rating >= 3.5) return 'text-warning';
    return 'text-danger';
  }
}
