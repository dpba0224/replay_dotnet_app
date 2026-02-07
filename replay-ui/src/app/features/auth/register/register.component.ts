import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  fullName = '';
  email = '';
  password = '';
  loading = false;
  error = '';
  success = '';

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    this.loading = true;
    this.error = '';
    this.success = '';

    this.authService.register({ email: this.email, password: this.password, fullName: this.fullName }).subscribe({
      next: (response) => {
        this.success = response.message || 'Registration successful! Please check your email to verify your account.';
        this.loading = false;
        setTimeout(() => {
          this.router.navigate(['/auth/verify-email'], { queryParams: { email: this.email } });
        }, 2000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Registration failed. Please try again.';
        this.loading = false;
      }
    });
  }
}
