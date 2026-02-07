import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToyService, Toy, ToyQueryParams, PagedResult } from '../../../core/services/toy.service';

@Component({
  selector: 'app-toy-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './toy-inventory.component.html',
  styleUrl: './toy-inventory.component.css'
})
export class ToyInventoryComponent implements OnInit {
  result = signal<PagedResult<Toy> | null>(null);
  loading = signal(false);
  error = signal('');

  searchTerm = '';
  showArchived = false;
  pageNumber = 1;
  pageSize = 20;

  Math = Math;

  private searchTimeout: any;

  constructor(public toyService: ToyService) {}

  ngOnInit(): void {
    this.loadToys();
  }

  loadToys(): void {
    this.loading.set(true);
    this.error.set('');

    const params: ToyQueryParams = {
      searchTerm: this.searchTerm || undefined,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      includeArchived: this.showArchived
    };

    this.toyService.getToys(params).subscribe({
      next: (result) => {
        this.result.set(result);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load toys');
        this.loading.set(false);
      }
    });
  }

  onSearch(): void {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.pageNumber = 1;
      this.loadToys();
    }, 300);
  }

  goToPage(page: number): void {
    this.pageNumber = page;
    this.loadToys();
  }

  archiveToy(toy: Toy): void {
    if (!confirm(`Are you sure you want to archive "${toy.name}"?`)) return;

    this.toyService.archiveToy(toy.id).subscribe({
      next: () => {
        this.loadToys();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to archive toy');
      }
    });
  }

  restoreToy(toy: Toy): void {
    this.toyService.restoreToy(toy.id).subscribe({
      next: () => {
        this.loadToys();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to restore toy');
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Available':
        return 'bg-green-100 text-green-800';
      case 'OnHold':
        return 'bg-yellow-100 text-yellow-800';
      case 'Traded':
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  getConditionBadgeClass(condition: number): string {
    if (condition >= 5) return 'bg-green-100 text-green-800';
    if (condition >= 4) return 'bg-blue-100 text-blue-800';
    if (condition >= 3) return 'bg-yellow-100 text-yellow-800';
    return 'bg-gray-100 text-gray-800';
  }
}
