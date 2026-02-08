import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToyService, Toy, ToyConditions } from '../../../core/services/toy.service';
import { AuthService } from '../../../core/services/auth.service';
import { TradeService, CreateTradeDto } from '../../../core/services/trade.service';
import { PaymentService } from '../../../core/services/payment.service';

@Component({
  selector: 'app-toy-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './toy-detail.component.html',
  styleUrl: './toy-detail.component.css'
})
export class ToyDetailComponent implements OnInit {
  toy = signal<Toy | null>(null);
  loading = signal(false);
  error = signal('');
  selectedImageIndex = signal(0);
  copied = signal(false);

  // Trade/Buy modal state
  showTradeModal = signal(false);
  tradeMode = signal<'trade' | 'buy'>('buy');
  tradeNotes = '';
  tradeSubmitting = signal(false);
  tradeError = signal('');
  tradeSuccess = signal('');

  conditions = ToyConditions;

  constructor(
    public toyService: ToyService,
    public authService: AuthService,
    private tradeService: TradeService,
    private paymentService: PaymentService,
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
    this.selectedImageIndex.set(0);

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

  openTradeModal(): void {
    this.tradeMode.set('trade');
    this.tradeNotes = '';
    this.tradeError.set('');
    this.tradeSuccess.set('');
    this.showTradeModal.set(true);
  }

  openBuyModal(): void {
    this.tradeMode.set('buy');
    this.tradeNotes = '';
    this.tradeError.set('');
    this.tradeSuccess.set('');
    this.showTradeModal.set(true);
  }

  closeTradeModal(): void {
    this.showTradeModal.set(false);
  }

  submitTrade(): void {
    const toy = this.toy();
    if (!toy) return;

    this.tradeSubmitting.set(true);
    this.tradeError.set('');

    const dto: CreateTradeDto = {
      requestedToyId: toy.id,
      tradeType: this.tradeMode() === 'trade' ? 0 : 1,
      notes: this.tradeNotes || undefined
    };

    this.tradeService.createTrade(dto).subscribe({
      next: (result) => {
        this.tradeSubmitting.set(false);
        if (result.succeeded && result.trade) {
          if (this.tradeMode() === 'buy') {
            // For purchase: create Stripe checkout session
            this.createCheckout(result.trade.id);
          } else {
            // For trade: show success and redirect
            this.tradeSuccess.set('Trade request submitted successfully! An admin will review it shortly.');
            setTimeout(() => {
              this.closeTradeModal();
              this.router.navigate(['/trades/history']);
            }, 2000);
          }
        } else {
          this.tradeError.set(result.message || 'Failed to create trade request.');
        }
      },
      error: (err) => {
        this.tradeSubmitting.set(false);
        this.tradeError.set(err.error?.message || 'Failed to create trade request.');
      }
    });
  }

  private createCheckout(tradeId: string): void {
    this.paymentService.createCheckoutSession(tradeId).subscribe({
      next: (result) => {
        this.tradeSubmitting.set(false);
        if (result.succeeded && result.checkoutUrl) {
          this.paymentService.redirectToCheckout(result.checkoutUrl);
        } else {
          this.tradeError.set(result.message || 'Failed to create checkout session.');
        }
      },
      error: (err) => {
        this.tradeSubmitting.set(false);
        this.tradeError.set(err.error?.message || 'Failed to start payment process.');
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
