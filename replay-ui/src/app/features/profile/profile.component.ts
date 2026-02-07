import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-6">My Profile</h1>
      @if (authService.currentUser(); as user) {
        <div class="bg-white shadow rounded-lg p-6">
          <div class="mb-4">
            <label class="block text-gray-700 font-medium">Full Name</label>
            <p class="text-gray-900">{{ user.fullName }}</p>
          </div>
          <div class="mb-4">
            <label class="block text-gray-700 font-medium">Email</label>
            <p class="text-gray-900">{{ user.email }}</p>
          </div>
          <div>
            <label class="block text-gray-700 font-medium">Role</label>
            <p class="text-gray-900">{{ user.role }}</p>
          </div>
        </div>
      }
    </div>
  `
})
export class ProfileComponent {
  constructor(public authService: AuthService) {}
}
