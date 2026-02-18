import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DealersService } from '../../services/dealers.service';
import { DealerDetails } from '../../models/dealer.model';

@Component({
  selector: 'app-dealer-details',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dealer-details.component.html',
  styleUrls: ['./dealer-details.component.scss']
})
export class DealerDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly dealersService = inject(DealersService);

  dealer: DealerDetails | null = null;
  loading = false;
  error: string | null = null;

  ngOnInit(): void {
    const dealerId = this.route.snapshot.paramMap.get('id');
    if (dealerId) {
      this.loadDealer(+dealerId);
    } else {
      this.error = 'ID дилера не указан';
    }
  }

  loadDealer(id: number): void {
    this.loading = true;
    this.dealersService.getDealerById(id).subscribe({
      next: (data) => {
        this.dealer = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Ошибка загрузки данных о дилере';
        this.loading = false;
        console.error(err);
      }
    });
  }

  getRatingClass(rating: number): string {
    if (rating >= 4.5) return 'text-success';
    if (rating >= 3.5) return 'text-warning';
    return 'text-danger';
  }
}
