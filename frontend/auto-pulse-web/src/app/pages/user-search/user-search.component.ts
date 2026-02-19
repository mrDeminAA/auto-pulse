import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { UserSearchService } from '../../services/user-search.service';
import { UserSearchRequest } from '../../models/user-search.model';

interface Brand {
  id: number;
  name: string;
}

interface Generation {
  value: string;
  label: string;
  years: string;
}

@Component({
  selector: 'app-user-search',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './user-search.component.html',
  styleUrls: ['./user-search.component.scss']
})
export class UserSearchComponent implements OnInit {
  private readonly userSearchService = inject(UserSearchService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  // Форма
  selectedBrandId: number | null = null;
  selectedModelId: number | null = null;
  selectedGeneration = '';
  yearFrom: number | null = null;
  yearTo: number | null = null;
  maxPrice: number | null = null;
  maxMileage: number | null = null;
  selectedRegions: string[] = ['china'];

  loading = false;
  saving = false;
  error: string | null = null;

  // Данные для выбора
  brands: Brand[] = [
    { id: 1, name: 'Audi' },
    { id: 2, name: 'BMW' },
    { id: 3, name: 'Mercedes-Benz' },
    { id: 4, name: 'Volkswagen' },
    { id: 5, name: 'Toyota' },
    { id: 6, name: 'Honda' },
    { id: 7, name: 'Nissan' },
    { id: 8, name: 'Mazda' },
    { id: 9, name: 'Lexus' },
    { id: 10, name: 'Porsche' }
  ];

  models: any[] = [];
  
  generations: Generation[] = [
    { value: '8Y', label: '8Y (2020-н.в.)', years: '2020-2024' },
    { value: '8V', label: '8V (2012-2020)', years: '2012-2020' },
    { value: '8P', label: '8P (2003-2013)', years: '2003-2013' },
    { value: 'any', label: 'Любое поколение', years: '' }
  ];

  regions = [
    { id: 'china', name: 'Китай', flag: '🇨🇳' },
    { id: 'europe', name: 'Европа', flag: '🇪🇺' },
    { id: 'usa', name: 'США', flag: '🇺🇸' },
    { id: 'korea', name: 'Корея', flag: '🇰🇷' }
  ];

  ngOnInit(): void {
    this.loadExistingSearch();
  }

  loadExistingSearch(): void {
    this.loading = true;
    this.cdr.detectChanges(); // Trigger change detection
    
    this.userSearchService.getUserSearch().subscribe({
      next: (search) => {
        this.selectedBrandId = search.brandId;
        this.selectedModelId = search.modelId;
        this.selectedGeneration = search.generation || '';
        this.yearFrom = search.yearFrom;
        this.yearTo = search.yearTo;
        this.maxPrice = search.maxPrice;
        this.maxMileage = search.maxMileage;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        // 404 - это нормально, поиск ещё не создан
        // Просто сбрасываем loading и оставляем форму пустой
        this.loading = false;
        this.cdr.detectChanges(); // Trigger change detection
        console.log('Поиск ещё не создан, форма пустая');
      }
    });
  }

  onBrandChange(): void {
    // Загрузка моделей для выбранного бренда (заглушка)
    this.models = [
      { id: 1, name: 'A3' },
      { id: 2, name: 'A4' },
      { id: 3, name: 'A4L' },
      { id: 4, name: 'A6' },
      { id: 5, name: 'A6L' },
      { id: 6, name: 'Q5' },
      { id: 7, name: 'Q7' }
    ];
    this.selectedModelId = null;
  }

  toggleRegion(regionId: string): void {
    const index = this.selectedRegions.indexOf(regionId);
    if (index > -1) {
      this.selectedRegions = this.selectedRegions.filter(r => r !== regionId);
    } else {
      this.selectedRegions = [...this.selectedRegions, regionId];
    }
  }

  get isValid(): boolean {
    return !!this.selectedBrandId && !!this.selectedModelId;
  }

  onSubmit(): void {
    if (!this.isValid) return;

    this.saving = true;
    this.error = null;

    const request: UserSearchRequest = {
      brandId: this.selectedBrandId || undefined,
      modelId: this.selectedModelId || undefined,
      generation: this.selectedGeneration || undefined,
      yearFrom: this.yearFrom || undefined,
      yearTo: this.yearTo || undefined,
      maxPrice: this.maxPrice || undefined,
      maxMileage: this.maxMileage || undefined,
      regions: JSON.stringify(this.selectedRegions)
    };

    this.userSearchService.saveUserSearch(request).subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error = err.message || 'Ошибка при сохранении';
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/dashboard']);
  }
}
