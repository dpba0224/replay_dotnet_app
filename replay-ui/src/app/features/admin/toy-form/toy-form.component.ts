import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToyService, Toy, CreateToyDto, UpdateToyDto, ToyCategories, ToyConditions, AgeGroups } from '../../../core/services/toy.service';

@Component({
  selector: 'app-toy-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="min-h-screen bg-gray-50 py-8">
      <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
        <!-- Header -->
        <div class="mb-8">
          <a routerLink="/admin/toys" class="inline-flex items-center text-indigo-600 hover:text-indigo-800 mb-4">
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
            </svg>
            Back to Inventory
          </a>
          <h1 class="text-3xl font-bold text-gray-900">
            {{ isEditMode() ? 'Edit Toy' : 'Add New Toy' }}
          </h1>
        </div>

        <!-- Loading State -->
        @if (loading()) {
          <div class="flex justify-center items-center py-12">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
          </div>
        }

        <!-- Form -->
        @if (!loading()) {
          <form (ngSubmit)="onSubmit()" class="bg-white rounded-lg shadow-lg p-6 space-y-6">
            <!-- Error Message -->
            @if (error()) {
              <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
                {{ error() }}
              </div>
            }

            <!-- Success Message -->
            @if (success()) {
              <div class="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-lg">
                {{ success() }}
              </div>
            }

            <!-- Name -->
            <div>
              <label for="name" class="block text-sm font-medium text-gray-700 mb-1">
                Toy Name <span class="text-red-500">*</span>
              </label>
              <input
                type="text"
                id="name"
                name="name"
                [(ngModel)]="formData.name"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                placeholder="e.g., LEGO Star Wars Millennium Falcon"
              />
            </div>

            <!-- Description -->
            <div>
              <label for="description" class="block text-sm font-medium text-gray-700 mb-1">
                Description <span class="text-red-500">*</span>
              </label>
              <textarea
                id="description"
                name="description"
                [(ngModel)]="formData.description"
                required
                rows="4"
                class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                placeholder="Describe the toy, its features, and what's included..."
              ></textarea>
            </div>

            <!-- Category & Age Group -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label for="category" class="block text-sm font-medium text-gray-700 mb-1">
                  Category <span class="text-red-500">*</span>
                </label>
                <select
                  id="category"
                  name="category"
                  [(ngModel)]="formData.category"
                  required
                  class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                >
                  @for (cat of categories; track cat.value) {
                    <option [ngValue]="cat.value">{{ cat.label }}</option>
                  }
                </select>
              </div>

              <div>
                <label for="ageGroup" class="block text-sm font-medium text-gray-700 mb-1">
                  Age Group <span class="text-red-500">*</span>
                </label>
                <select
                  id="ageGroup"
                  name="ageGroup"
                  [(ngModel)]="formData.ageGroup"
                  required
                  class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                >
                  @for (age of ageGroups; track age) {
                    <option [value]="age">{{ age }}</option>
                  }
                </select>
              </div>
            </div>

            <!-- Condition & Price -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label for="condition" class="block text-sm font-medium text-gray-700 mb-1">
                  Condition <span class="text-red-500">*</span>
                </label>
                <select
                  id="condition"
                  name="condition"
                  [(ngModel)]="formData.condition"
                  required
                  class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                >
                  @for (cond of conditions; track cond.value) {
                    <option [ngValue]="cond.value">{{ cond.label }} - {{ cond.description }}</option>
                  }
                </select>
              </div>

              <div>
                <label for="price" class="block text-sm font-medium text-gray-700 mb-1">
                  Price ($) <span class="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  id="price"
                  name="price"
                  [(ngModel)]="formData.price"
                  required
                  min="0"
                  step="0.01"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  placeholder="29.99"
                />
              </div>
            </div>

            <!-- Image Upload (only for edit mode) -->
            @if (isEditMode() && toy()) {
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Images</label>

                <!-- Current Images -->
                @if (toy()!.images.length > 0) {
                  <div class="grid grid-cols-4 gap-4 mb-4">
                    @for (image of toy()!.images; track image.id) {
                      <div class="relative group">
                        <img
                          [src]="toyService.getImageUrl(image.imagePath)"
                          [alt]="toy()!.name"
                          class="w-full aspect-square object-cover rounded-lg"
                        />
                        <button
                          type="button"
                          (click)="deleteImage(image.id)"
                          class="absolute top-2 right-2 bg-red-500 text-white p-1 rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                        >
                          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                          </svg>
                        </button>
                      </div>
                    }
                  </div>
                }

                <!-- Upload New Image -->
                <div class="flex items-center gap-4">
                  <input
                    type="file"
                    (change)="onFileSelected($event)"
                    accept="image/jpeg,image/png,image/webp"
                    class="hidden"
                    #fileInput
                  />
                  <button
                    type="button"
                    (click)="fileInput.click()"
                    [disabled]="uploading()"
                    class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                  >
                    {{ uploading() ? 'Uploading...' : 'Add Image' }}
                  </button>
                  <span class="text-sm text-gray-500">JPEG, PNG, or WebP. Max 5MB.</span>
                </div>
              </div>
            }

            <!-- Submit Button -->
            <div class="flex justify-end gap-4 pt-4">
              <a
                routerLink="/admin/toys"
                class="px-6 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </a>
              <button
                type="submit"
                [disabled]="saving()"
                class="px-6 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 disabled:opacity-50"
              >
                {{ saving() ? 'Saving...' : (isEditMode() ? 'Save Changes' : 'Create Toy') }}
              </button>
            </div>
          </form>
        }
      </div>
    </div>
  `
})
export class ToyFormComponent implements OnInit {
  isEditMode = signal(false);
  toyId = signal<string | null>(null);
  toy = signal<Toy | null>(null);
  loading = signal(false);
  saving = signal(false);
  uploading = signal(false);
  error = signal('');
  success = signal('');

  formData: CreateToyDto = {
    name: '',
    description: '',
    category: 0,
    ageGroup: '3-5',
    condition: 3,
    price: 0
  };

  categories = ToyCategories;
  conditions = ToyConditions;
  ageGroups = AgeGroups;

  constructor(
    public toyService: ToyService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.toyId.set(id);
      this.loadToy(id);
    }
  }

  loadToy(id: string): void {
    this.loading.set(true);
    this.toyService.getToyById(id).subscribe({
      next: (toy) => {
        this.toy.set(toy);
        this.formData = {
          name: toy.name,
          description: toy.description,
          category: this.getCategoryValue(toy.category),
          ageGroup: toy.ageGroup,
          condition: toy.condition,
          price: toy.price
        };
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load toy');
        this.loading.set(false);
      }
    });
  }

  getCategoryValue(categoryName: string): number {
    const cat = this.categories.find(c =>
      c.label.replace(/\s+&\s+/g, 'And').replace(/\s+/g, '') === categoryName
    );
    return cat?.value ?? 0;
  }

  onSubmit(): void {
    this.saving.set(true);
    this.error.set('');
    this.success.set('');

    if (this.isEditMode()) {
      const updateDto: UpdateToyDto = { ...this.formData };
      this.toyService.updateToy(this.toyId()!, updateDto).subscribe({
        next: () => {
          this.success.set('Toy updated successfully!');
          this.saving.set(false);
          this.loadToy(this.toyId()!);
        },
        error: (err) => {
          this.error.set(err.error?.message || 'Failed to update toy');
          this.saving.set(false);
        }
      });
    } else {
      this.toyService.createToy(this.formData).subscribe({
        next: (toy) => {
          this.success.set('Toy created successfully!');
          this.saving.set(false);
          setTimeout(() => {
            this.router.navigate(['/admin/toys', toy.id, 'edit']);
          }, 1000);
        },
        error: (err) => {
          this.error.set(err.error?.message || 'Failed to create toy');
          this.saving.set(false);
        }
      });
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length || !this.toyId()) return;

    const file = input.files[0];
    this.uploading.set(true);

    this.toyService.uploadImage(this.toyId()!, file).subscribe({
      next: () => {
        this.uploading.set(false);
        this.loadToy(this.toyId()!);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to upload image');
        this.uploading.set(false);
      }
    });

    input.value = '';
  }

  deleteImage(imageId: string): void {
    if (!this.toyId()) return;

    this.toyService.deleteImage(this.toyId()!, imageId).subscribe({
      next: () => {
        this.loadToy(this.toyId()!);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to delete image');
      }
    });
  }
}
