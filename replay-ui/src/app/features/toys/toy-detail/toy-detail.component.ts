import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToyService, Toy, ToyConditions } from '../../../core/services/toy.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-toy-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './toy-detail.component.html',
  styleUrl: './toy-detail.component.css'
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
