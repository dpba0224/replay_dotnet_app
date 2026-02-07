import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToyService, Toy, ToyQueryParams, PagedResult } from '../../../core/services/toy.service';

@Component({
  selector: 'app-toy-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="min-h-screen bg-gray-50 py-8">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
          <div>
            <h1 class="text-3xl font-bold text-gray-900">Toy Inventory</h1>
            <p class="text-gray-600 mt-1">Manage your toy collection</p>
          </div>
          <a
            routerLink="/admin/toys/new"
            class="inline-flex items-center px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors"
          >
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            Add New Toy
          </a>
        </div>

        <!-- Filters -->
        <div class="bg-white rounded-lg shadow p-4 mb-6">
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="flex-1">
              <input
                type="text"
                [(ngModel)]="searchTerm"
                (input)="onSearch()"
                placeholder="Search toys..."
                class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>
            <div class="flex gap-4">
              <label class="flex items-center gap-2">
                <input
                  type="checkbox"
                  [(ngModel)]="showArchived"
                  (change)="loadToys()"
                  class="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                />
                <span class="text-sm text-gray-700">Show Archived</span>
              </label>
            </div>
          </div>
        </div>

        <!-- Loading State -->
        @if (loading()) {
          <div class="flex justify-center items-center py-12">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
          </div>
        }

        <!-- Error State -->
        @if (error()) {
          <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
            {{ error() }}
          </div>
        }

        <!-- Toys Table -->
        @if (!loading() && result()) {
          <div class="bg-white rounded-lg shadow overflow-hidden">
            @if (result()!.items.length === 0) {
              <div class="text-center py-12">
                <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                </svg>
                <h3 class="mt-2 text-sm font-medium text-gray-900">No toys found</h3>
                <p class="mt-1 text-sm text-gray-500">Get started by adding a new toy.</p>
                <div class="mt-6">
                  <a
                    routerLink="/admin/toys/new"
                    class="inline-flex items-center px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
                  >
                    Add New Toy
                  </a>
                </div>
              </div>
            } @else {
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Toy</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Category</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Condition</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Price</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  @for (toy of result()!.items; track toy.id) {
                    <tr [class.bg-gray-50]="toy.isArchived">
                      <td class="px-6 py-4 whitespace-nowrap">
                        <div class="flex items-center">
                          <div class="h-12 w-12 flex-shrink-0">
                            @if (toy.images.length > 0) {
                              <img
                                [src]="toyService.getImageUrl(toy.images[0].imagePath)"
                                [alt]="toy.name"
                                class="h-12 w-12 rounded-lg object-cover"
                              />
                            } @else {
                              <div class="h-12 w-12 rounded-lg bg-gray-200 flex items-center justify-center">
                                <svg class="h-6 w-6 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                </svg>
                              </div>
                            }
                          </div>
                          <div class="ml-4">
                            <div class="text-sm font-medium text-gray-900">{{ toy.name }}</div>
                            <div class="text-sm text-gray-500">{{ toy.ageGroup }}</div>
                          </div>
                        </div>
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {{ toyService.getCategoryLabel(toy.category) }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap">
                        <span
                          [class]="getConditionBadgeClass(toy.condition)"
                          class="px-2 py-1 text-xs font-medium rounded"
                        >
                          {{ toy.conditionLabel }}
                        </span>
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                        \${{ toy.price.toFixed(2) }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap">
                        @if (toy.isArchived) {
                          <span class="px-2 py-1 text-xs font-medium rounded bg-gray-100 text-gray-800">
                            Archived
                          </span>
                        } @else {
                          <span
                            [class]="getStatusBadgeClass(toy.status)"
                            class="px-2 py-1 text-xs font-medium rounded-full"
                          >
                            {{ toy.status }}
                          </span>
                        }
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <div class="flex justify-end gap-2">
                          <a
                            [routerLink]="['/admin/toys', toy.id, 'edit']"
                            class="text-indigo-600 hover:text-indigo-900"
                          >
                            Edit
                          </a>
                          @if (toy.isArchived) {
                            <button
                              (click)="restoreToy(toy)"
                              class="text-green-600 hover:text-green-900"
                            >
                              Restore
                            </button>
                          } @else {
                            <button
                              (click)="archiveToy(toy)"
                              class="text-red-600 hover:text-red-900"
                            >
                              Archive
                            </button>
                          }
                        </div>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>

              <!-- Pagination -->
              @if (result()!.totalPages > 1) {
                <div class="px-6 py-4 border-t border-gray-200 flex justify-between items-center">
                  <p class="text-sm text-gray-600">
                    Showing {{ (result()!.pageNumber - 1) * result()!.pageSize + 1 }} to
                    {{ Math.min(result()!.pageNumber * result()!.pageSize, result()!.totalCount) }}
                    of {{ result()!.totalCount }} toys
                  </p>
                  <div class="flex gap-2">
                    <button
                      (click)="goToPage(pageNumber - 1)"
                      [disabled]="!result()!.hasPreviousPage"
                      class="px-3 py-1 border border-gray-300 rounded-md text-sm text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Previous
                    </button>
                    <button
                      (click)="goToPage(pageNumber + 1)"
                      [disabled]="!result()!.hasNextPage"
                      class="px-3 py-1 border border-gray-300 rounded-md text-sm text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Next
                    </button>
                  </div>
                </div>
              }
            }
          </div>
        }
      </div>
    </div>
  `
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
