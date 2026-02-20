import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard-layout">
      <main class="dashboard-layout__content">
        <ng-content></ng-content>
      </main>
    </div>
  `,
  styles: [`
    .dashboard-layout {
      min-height: 100vh;
      background: #f3f4f6;
    }

    .dashboard-layout__content {
      padding: 32px 24px;
      max-width: 1200px;
      margin: 0 auto;
    }
  `]
})
export class DashboardLayoutComponent {
}
