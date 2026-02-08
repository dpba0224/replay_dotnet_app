import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToyService, Toy, ToyQueryParams, ToyCategories, ToyConditions, AgeGroups, PagedResult } from '../../../core/services/toy.service';

@Component({
  selector: 'app-toy-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './toy-list.component.html',
  styleUrl: './toy-list.component.css'
})
export class ToyListComponent implements OnInit {
  result = signal<PagedResult<Toy> | null>(null);
  loading = signal(false);
  error = signal('');
  showFilters = signal(false);

  filters: ToyQueryParams = {
    pageNumber: 1,
    pageSize: 12,
    sortBy: 'newest'
  };

  categories = ToyCategories;
  conditions = ToyConditions;
  ageGroups = AgeGroups;

  private searchTimeout: any;

  constructor(public toyService: ToyService) {}

  ngOnInit(): void {
    this.loadToys();
  }

  loadToys(): void {
    this.loading.set(true);
    this.error.set('');

    this.toyService.getToys(this.filters).subscribe({
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
      this.filters.pageNumber = 1;
      this.loadToys();
    }, 300);
  }

  onFilterChange(): void {
    this.filters.pageNumber = 1;
    this.loadToys();
  }

  clearFilters(): void {
    this.filters = {
      pageNumber: 1,
      pageSize: 12,
      sortBy: 'newest'
    };
    this.loadToys();
  }

  toggleFilters(): void {
    this.showFilters.update(v => !v);
  }

  hasActiveFilters(): boolean {
    return !!(
      this.filters.searchTerm ||
      this.filters.category ||
      this.filters.minCondition ||
      this.filters.ageGroup ||
      this.filters.minPrice ||
      this.filters.maxPrice ||
      this.filters.status
    );
  }

  goToPage(page: number): void {
    this.filters.pageNumber = page;
    this.loadToys();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Available':
        return 'bg-green-100 text-green-800';
      case 'OnHold':
        return 'bg-yellow-100 text-yellow-800';
      case 'Traded':
        return 'bg-blue-100 text-blue-800';
      case 'Sold':
        return 'bg-purple-100 text-purple-800';
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
