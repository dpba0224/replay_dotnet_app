import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  email = '';
  loading = false;
  error = '';
  submitted = false;

  constructor(private authService: AuthService) {}

  onSubmit(): void {
    this.loading = true;
    this.error = '';

    this.authService.forgotPassword(this.email).subscribe({
      next: () => {
        this.submitted = true;
        this.loading = false;
      },
      error: () => {
        // Still show success message for security (don't reveal if email exists)
        this.submitted = true;
        this.loading = false;
      }
    });
  }
}
