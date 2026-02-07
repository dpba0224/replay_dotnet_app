import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToyService, Toy, CreateToyDto, UpdateToyDto, ToyCategories, ToyConditions, AgeGroups } from '../../../core/services/toy.service';

@Component({
  selector: 'app-toy-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './toy-form.component.html',
  styleUrl: './toy-form.component.css'
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
