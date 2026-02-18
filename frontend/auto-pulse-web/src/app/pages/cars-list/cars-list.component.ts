import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MainLayoutComponent, Breadcrumb } from '../../shared/components/main-layout/main-layout.component';
import { SidebarComponent } from '../../shared/components/sidebar/sidebar.component';
import { CarsStore } from '../../stores/cars.store';
import { CarDto } from '../../models/dealer.model';

@Component({
  selector: 'app-cars-list',
  standalone: true,
  imports: [CommonModule, FormsModule, MainLayoutComponent, SidebarComponent],
  templateUrl: './cars-list.component.html',
  styleUrls: ['./cars-list.component.scss']
})
export class CarsListComponent implements OnInit {
  private readonly carsStore = inject(CarsStore);

  readonly cars = this.carsStore.cars;
  readonly loading = this.carsStore.loading;
  readonly error = this.carsStore.error;
  readonly totalCount = this.carsStore.totalCount;

  // Хлебные крошки
  breadcrumbs: Breadcrumb[] = [
    { label: 'Автомобили', link: null }
  ];

  // Фильтры
  selectedBrand: string | null = null;
  yearFrom: number | null = null;
  yearTo: number | null = null;
  priceFrom: number | null = null;
  priceTo: number | null = null;
  onlyAvailable = false;
  sortBy = 'newest';
  searchQuery = '';

  // Заготовки для будущего функционала
  brands = ['Audi', 'BMW', 'Mercedes-Benz', 'Volkswagen', 'Toyota'];

  ngOnInit(): void {
    this.carsStore.loadCars();
  }

  trackByCarId(index: number, car: CarDto): number {
    return car.id;
  }

  getSortLabel(): string {
    const labels: Record<string, string> = {
      'newest': 'По релевантности',
      'oldest': 'Сначала старые',
      'price-asc': 'По возрастанию цены',
      'price-desc': 'По убыванию цены',
      'year-desc': 'По году (новые)'
    };
    return labels[this.sortBy] || 'По релевантности';
  }

  onSearch(query: string): void {
    this.searchQuery = query;
    this.carsStore.loadCars();
  }

  onReset(): void {
    this.selectedBrand = null;
    this.yearFrom = null;
    this.yearTo = null;
    this.priceFrom = null;
    this.priceTo = null;
    this.onlyAvailable = false;
    this.sortBy = 'newest';
    this.searchQuery = '';
    this.carsStore.loadCars();
  }

  onFilterChange(): void {
    // Здесь будет логика фильтрации
    console.log('Фильтры изменены', {
      brand: this.selectedBrand,
      yearFrom: this.yearFrom,
      yearTo: this.yearTo,
      priceFrom: this.priceFrom,
      priceTo: this.priceTo,
      onlyAvailable: this.onlyAvailable,
      sortBy: this.sortBy
    });
    this.carsStore.loadCars();
  }

  toggleMobileFilters(): void {
    // Для мобильной версии
    console.log('Toggle mobile filters');
  }
}
