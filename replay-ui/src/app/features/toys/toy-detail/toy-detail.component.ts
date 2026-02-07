import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToyService, Toy, ToyConditions } from '../../../core/services/toy.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-toy-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="bg-gray-50 min-h-screen py-8">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <!-- Back Button -->
        <a routerLink="/" class="inline-flex items-center text-indigo-600 hover:text-indigo-800 mb-6">
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          Back to Browse
        </a>

        <!-- Loading State -->
        @if (loading()) {
          <div class="flex justify-center items-center py-12">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
          </div>
        }

        <!-- Error State -->
        @if (error()) {
          <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
            {{ error() }}
          </div>
        }

        <!-- Toy Details -->
        @if (!loading() && toy()) {
          <div class="bg-white rounded-lg shadow-lg overflow-hidden">
            <div class="lg:flex">
              <!-- Image Gallery -->
              <div class="lg:w-1/2">
                <div class="aspect-square bg-gray-100 relative">
                  @if (toy()!.images.length > 0) {
                    <img
                      [src]="toyService.getImageUrl(toy()!.images[selectedImageIndex()].imagePath)"
                      [alt]="toy()!.name"
                      class="w-full h-full object-cover"
                    />
                  } @else {
                    <div class="w-full h-full flex items-center justify-center text-gray-400">
                      <svg class="h-24 w-24" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                    </div>
                  }
                </div>

                <!-- Thumbnail Gallery -->
                @if (toy()!.images.length > 1) {
                  <div class="flex gap-2 p-4 overflow-x-auto">
                    @for (image of toy()!.images; track image.id; let i = $index) {
                      <button
                        (click)="selectedImageIndex.set(i)"
                        [class.ring-2]="selectedImageIndex() === i"
                        [class.ring-indigo-500]="selectedImageIndex() === i"
                        class="flex-shrink-0 w-20 h-20 rounded-lg overflow-hidden"
                      >
                        <img
                          [src]="toyService.getImageUrl(image.imagePath)"
                          [alt]="toy()!.name"
                          class="w-full h-full object-cover"
                        />
                      </button>
                    }
                  </div>
                }
              </div>

              <!-- Details -->
              <div class="lg:w-1/2 p-6 lg:p-8">
                <!-- Status Badge -->
                <div class="mb-4">
                  <span
                    [class]="getStatusBadgeClass(toy()!.status)"
                    class="px-3 py-1 text-sm font-medium rounded-full"
                  >
                    {{ toy()!.status }}
                  </span>
                </div>

                <!-- Title -->
                <h1 class="text-3xl font-bold text-gray-900 mb-4">{{ toy()!.name }}</h1>

                <!-- Price -->
                <div class="text-3xl font-bold text-indigo-600 mb-6">
                  \${{ toy()!.price.toFixed(2) }}
                </div>

                <!-- Meta Info -->
                <div class="grid grid-cols-2 gap-4 mb-6">
                  <div>
                    <span class="text-sm text-gray-500">Category</span>
                    <p class="font-medium">{{ toyService.getCategoryLabel(toy()!.category) }}</p>
                  </div>
                  <div>
                    <span class="text-sm text-gray-500">Age Group</span>
                    <p class="font-medium">{{ toy()!.ageGroup }}</p>
                  </div>
                  <div>
                    <span class="text-sm text-gray-500">Condition</span>
                    <div class="flex items-center gap-2">
                      <span
                        [class]="getConditionBadgeClass(toy()!.condition)"
                        class="px-2 py-1 text-sm font-medium rounded"
                      >
                        {{ toy()!.conditionLabel }}
                      </span>
                      <span class="text-gray-500 text-sm">{{ getConditionDescription(toy()!.condition) }}</span>
                    </div>
                  </div>
                  <div>
                    <span class="text-sm text-gray-500">Added</span>
                    <p class="font-medium">{{ formatDate(toy()!.createdAt) }}</p>
                  </div>
                </div>

                <!-- Description -->
                <div class="mb-8">
                  <h2 class="text-lg font-semibold text-gray-900 mb-2">Description</h2>
                  <p class="text-gray-600 whitespace-pre-line">{{ toy()!.description }}</p>
                </div>

                <!-- Action Buttons -->
                @if (toy()!.status === 'Available') {
                  <div class="space-y-3">
                    @if (authService.isAuthenticated()) {
                      <button
                        class="w-full py-3 px-4 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 transition-colors"
                      >
                        Trade for This Toy
                      </button>
                      <button
                        class="w-full py-3 px-4 border-2 border-indigo-600 text-indigo-600 font-medium rounded-lg hover:bg-indigo-50 transition-colors"
                      >
                        Buy This Toy - \${{ toy()!.price.toFixed(2) }}
                      </button>
                    } @else {
                      <a
                        routerLink="/auth/login"
                        class="block w-full py-3 px-4 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 transition-colors text-center"
                      >
                        Sign in to Trade or Buy
                      </a>
                    }
                  </div>
                } @else {
                  <div class="bg-gray-100 rounded-lg p-4 text-center text-gray-600">
                    This toy is currently not available for trading or purchase.
                  </div>
                }

                <!-- Share Link -->
                <div class="mt-6 pt-6 border-t border-gray-200">
                  <p class="text-sm text-gray-500 mb-2">Share this toy:</p>
                  <div class="flex items-center gap-2">
                    <input
                      type="text"
                      [value]="getShareUrl()"
                      readonly
                      class="flex-1 px-3 py-2 border border-gray-300 rounded-md text-sm bg-gray-50"
                    />
                    <button
                      (click)="copyShareUrl()"
                      class="px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 transition-colors"
                    >
                      {{ copied() ? 'Copied!' : 'Copy' }}
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class ToyDetailComponent implements OnInit {
  toy = signal<Toy | null>(null);
  loading = signal(false);
  error = signal('');
  selectedImageIndex = signal(0);
  copied = signal(false);

  conditions = ToyConditions;

  constructor(
    public toyService: ToyService,
    public authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadToy(id);
    }
  }

  loadToy(id: string): void {
    this.loading.set(true);
    this.error.set('');

    this.toyService.getToyById(id).subscribe({
      next: (toy) => {
        this.toy.set(toy);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load toy');
        this.loading.set(false);
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

  getConditionDescription(condition: number): string {
    const cond = this.conditions.find(c => c.value === condition);
    return cond?.description || '';
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getShareUrl(): string {
    return window.location.href;
  }

  copyShareUrl(): void {
    navigator.clipboard.writeText(this.getShareUrl()).then(() => {
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    });
  }
}
