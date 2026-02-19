import { Component, inject, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthStore } from '../../stores/auth.store';
import { RegisterRequest } from '../../models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  name = '';
  email = '';
  password = '';
  confirmPassword = '';
  agreeTerms = false;
  showPassword = false;

  readonly loading = this.authStore.loading;
  readonly error = this.authStore.error;
  readonly isAuthenticated = this.authStore.isAuthenticated;

  constructor() {
    // Effect для редиректа после успешной регистрации
    effect(() => {
      if (this.isAuthenticated()) {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  ngOnInit(): void {
    this.authStore.clearError();
  }

  get passwordMismatch(): boolean {
    return !!(this.password && this.confirmPassword && this.password !== this.confirmPassword);
  }

  get isValid(): boolean {
    return (
      !!this.name.trim() &&
      !!this.email.trim() &&
      this.password.length >= 6 &&
      !this.passwordMismatch &&
      this.agreeTerms
    );
  }

  onSubmit(): void {
    if (!this.isValid) {
      return;
    }

    const request: RegisterRequest = {
      email: this.email.toLowerCase().trim(),
      password: this.password,
      name: this.name.trim()
    };

    this.authStore.register(request);
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
