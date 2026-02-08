import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToyService, Toy } from '../../../core/services/toy.service';
import { ReturnService, ReturnDto, ReturnQueryParams } from '../../../core/services/return.service';

@Component({
  selector: 'app-my-toys',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './my-toys.component.html',
  styleUrl: './my-toys.component.css'
})
export class MyToysComponent implements OnInit {
  toys = signal<Toy[]>([]);
  returns = signal<ReturnDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  activeTab = signal<'held' | 'returns'>('held');

  // Return modal state
  showReturnModal = signal(false);
  selectedToy = signal<Toy | null>(null);
  returnNotes = '';
  returnSubmitting = signal(false);
  returnError = signal<string | null>(null);
  returnSuccess = signal<string | null>(null);

  constructor(
    public toyService: ToyService,
    public returnService: ReturnService
  ) {}

  ngOnInit(): void {
    this.loadMyToys();
    this.loadMyReturns();
  }

  switchTab(tab: 'held' | 'returns'): void {
    this.activeTab.set(tab);
  }

  loadMyToys(): void {
    this.loading.set(true);
    this.error.set(null);
    this.toyService.getMyToys().subscribe({
      next: (toys) => {
        this.toys.set(toys);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load your toys.');
        this.loading.set(false);
      }
    });
  }

  loadMyReturns(): void {
    this.returnService.getUserReturns({ pageSize: 50 }).subscribe({
      next: (result) => {
        this.returns.set(result.items);
      },
      error: () => {}
    });
  }

  openReturnModal(toy: Toy): void {
    this.selectedToy.set(toy);
    this.returnNotes = '';
    this.returnError.set(null);
    this.returnSuccess.set(null);
    this.showReturnModal.set(true);
  }

  closeReturnModal(): void {
    this.showReturnModal.set(false);
    this.selectedToy.set(null);
  }

  submitReturn(): void {
    const toy = this.selectedToy();
    if (!toy) return;

    this.returnSubmitting.set(true);
    this.returnError.set(null);

    this.returnService.initiateReturn({
      toyId: toy.id,
      userNotes: this.returnNotes || undefined
    }).subscribe({
      next: (result) => {
        this.returnSubmitting.set(false);
        if (result.succeeded) {
          this.returnSuccess.set(result.message || 'Return request submitted successfully.');
          this.loadMyToys();
          this.loadMyReturns();
          setTimeout(() => this.closeReturnModal(), 2000);
        } else {
          this.returnError.set(result.message || 'Failed to submit return request.');
        }
      },
      error: (err) => {
        this.returnSubmitting.set(false);
        this.returnError.set(err.error?.message || 'Failed to submit return request.');
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Available': return 'bg-green-100 text-green-800';
      case 'Traded': return 'bg-blue-100 text-blue-800';
      case 'Sold': return 'bg-purple-100 text-purple-800';
      case 'PendingReturn': return 'bg-yellow-100 text-yellow-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  getConditionBadgeClass(condition: number): string {
    if (condition >= 4) return 'bg-green-50 text-green-700';
    if (condition >= 3) return 'bg-blue-50 text-blue-700';
    if (condition >= 2) return 'bg-yellow-50 text-yellow-700';
    return 'bg-red-50 text-red-700';
  }

  canReturn(toy: Toy): boolean {
    return toy.status === 'Traded' || toy.status === 'Sold';
  }
}
