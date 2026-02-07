import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-email-verify',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './email-verify.component.html',
  styleUrl: './email-verify.component.css'
})
export class EmailVerifyComponent {
  email = '';
  code = '';
  loading = false;
  error = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
    });
  }

  onSubmit(): void {
    this.loading = true;
    this.error = '';

    this.authService.verifyEmail(this.email, this.code).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.error = err.error?.message || 'Verification failed. Please try again.';
        this.loading = false;
      }
    });
  }
}
