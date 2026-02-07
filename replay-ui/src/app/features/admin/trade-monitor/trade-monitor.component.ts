import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-trade-monitor',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-6">Trade Monitor</h1>
      <p class="text-gray-600">Monitor and manage trades here.</p>
    </div>
  `
})
export class TradeMonitorComponent {}
