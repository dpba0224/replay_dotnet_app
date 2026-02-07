import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-6">Messages</h1>
      <p class="text-gray-600">Your conversations will appear here.</p>
    </div>
  `
})
export class MessagesComponent {}
