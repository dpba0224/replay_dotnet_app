import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-6">User Management</h1>
      <p class="text-gray-600">Manage platform users here.</p>
    </div>
  `
})
export class UserManagementComponent {}
