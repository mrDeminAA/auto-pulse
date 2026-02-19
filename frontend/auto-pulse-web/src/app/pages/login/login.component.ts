import { Component, inject, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthStore } from '../../stores/auth.store';
import { LoginRequest } from '../../models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  email = '';
  password = '';
  rememberMe = false;
  showPassword = false;

  readonly loading = this.authStore.loading;
  readonly error = this.authStore.error;
  readonly isAuthenticated = this.authStore.isAuthenticated;

  constructor() {
    // Effect для редиректа после успешной авторизации
    effect(() => {
      if (this.isAuthenticated()) {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  ngOnInit(): void {
    this.authStore.clearError();
  }

  onSubmit(): void {
    if (!this.email || !this.password) {
      return;
    }

    const request: LoginRequest = {
      email: this.email.toLowerCase().trim(),
      password: this.password
    };

    this.authStore.login(request);
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
