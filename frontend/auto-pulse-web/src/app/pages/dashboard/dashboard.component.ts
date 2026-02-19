import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthStore } from '../../stores/auth.store';
import { UserSearchService } from '../../services/user-search.service';
import { UserSearchResponse } from '../../models/user-search.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  private readonly authStore = inject(AuthStore);
  private readonly userSearchService = inject(UserSearchService);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly authStorePublic = this.authStore;
  readonly user = this.authStore.user;
  readonly userName = this.authStore.userName;

  userSearch: UserSearchResponse | null = null;
  loading = false;
  error: string | null = null;

  stats = {
    totalCars: 0,
    newToday: 0,
    favorites: 0,
    alerts: 0
  };

  ngOnInit(): void {
    // Проверяем авторизацию перед загрузкой данных
    if (!this.authStore.isAuthenticated()) {
      return;
    }
    this.loadUserSearch();
  }

  loadUserSearch(): void {
    this.loading = true;
    this.cdr.detectChanges();
    
    this.userSearchService.getUserSearch().subscribe({
      next: (search) => {
        this.userSearch = search;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        // 404 - это нормально, поиск ещё не создан
        if (err.status === 404) {
          this.userSearch = null;
        }
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  get hasActiveSearch(): boolean {
    return this.userSearch?.status === 'Active';
  }

  get searchProgress(): string {
    if (!this.userSearch) return '0%';
    // Заглушка для прогресса
    return '0%';
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      'Active': 'Активен',
      'Paused': 'На паузе',
      'Completed': 'Завершён',
      'Cancelled': 'Отменён'
    };
    return labels[status] || status;
  }
}
