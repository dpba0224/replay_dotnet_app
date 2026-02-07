import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-trade-history',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-6">Trade History</h1>
      <p class="text-gray-600">View your past trades and purchases.</p>
    </div>
  `
})
export class TradeHistoryComponent {}
