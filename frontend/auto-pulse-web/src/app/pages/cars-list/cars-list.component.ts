import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CarsStore } from '../../stores/cars.store';
import { CarDto } from '../../models/dealer.model';

@Component({
  selector: 'app-cars-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cars-list.component.html',
  styleUrls: ['./cars-list.component.scss']
})
export class CarsListComponent implements OnInit {
  private readonly carsStore = inject(CarsStore);

  readonly cars = this.carsStore.cars;
  readonly loading = this.carsStore.loading;
  readonly error = this.carsStore.error;
  readonly totalCount = this.carsStore.totalCount;

  ngOnInit(): void {
    this.carsStore.loadCars();
  }

  trackByCarId(index: number, car: CarDto): number {
    return car.id;
  }
}
