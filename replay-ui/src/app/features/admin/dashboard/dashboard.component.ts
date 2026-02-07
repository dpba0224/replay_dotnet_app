import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-6">Admin Dashboard</h1>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div class="bg-white shadow rounded-lg p-6">
          <h3 class="text-lg font-semibold text-gray-700">Total Users</h3>
          <p class="text-3xl font-bold text-indigo-600">0</p>
        </div>
        <div class="bg-white shadow rounded-lg p-6">
          <h3 class="text-lg font-semibold text-gray-700">Active Toys</h3>
          <p class="text-3xl font-bold text-green-600">0</p>
        </div>
        <div class="bg-white shadow rounded-lg p-6">
          <h3 class="text-lg font-semibold text-gray-700">Pending Trades</h3>
          <p class="text-3xl font-bold text-yellow-600">0</p>
        </div>
        <div class="bg-white shadow rounded-lg p-6">
          <h3 class="text-lg font-semibold text-gray-700">Total Revenue</h3>
          <p class="text-3xl font-bold text-blue-600">$0</p>
        </div>
      </div>
      <div class="mt-8 grid grid-cols-1 md:grid-cols-3 gap-4">
        <a routerLink="/admin/users" class="bg-indigo-600 text-white p-4 rounded-lg text-center hover:bg-indigo-700">
          Manage Users
        </a>
        <a routerLink="/admin/toys" class="bg-green-600 text-white p-4 rounded-lg text-center hover:bg-green-700">
          Manage Toys
        </a>
        <a routerLink="/admin/trades" class="bg-yellow-600 text-white p-4 rounded-lg text-center hover:bg-yellow-700">
          Monitor Trades
        </a>
      </div>
    </div>
  `
})
export class DashboardComponent {}
