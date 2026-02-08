import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TradeService, TradeDto } from '../../../core/services/trade.service';
import { ToyService } from '../../../core/services/toy.service';

@Component({
  selector: 'app-trade-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './trade-detail.component.html',
  styleUrl: './trade-detail.component.css'
})
export class TradeDetailComponent implements OnInit {
  trade = signal<TradeDto | null>(null);
  loading = signal(true);
  error = signal('');
  cancelling = signal(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public tradeService: TradeService,
    public toyService: ToyService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadTrade(id);
    }
  }

  private loadTrade(id: string): void {
    this.tradeService.getTradeById(id).subscribe({
      next: (trade) => {
        this.trade.set(trade);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load trade.');
        this.loading.set(false);
      }
    });
  }

  cancelTrade(): void {
    const trade = this.trade();
    if (!trade) return;

    this.cancelling.set(true);
    this.tradeService.cancelTrade(trade.id).subscribe({
      next: (result) => {
        this.cancelling.set(false);
        if (result.succeeded && result.trade) {
          this.trade.set(result.trade);
        }
      },
      error: (err) => {
        this.cancelling.set(false);
        this.error.set(err.error?.message || 'Failed to cancel trade.');
      }
    });
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
