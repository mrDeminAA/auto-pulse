import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStore } from './stores/auth.store';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly authStore = inject(AuthStore);
  
  protected readonly isAuthenticated = this.authStore.isAuthenticated;
  protected readonly userName = this.authStore.userName;

  protected logout(): void {
    this.authStore.logout();
  }
}
