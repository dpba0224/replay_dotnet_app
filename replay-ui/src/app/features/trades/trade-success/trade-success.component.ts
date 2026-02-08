import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TradeService, TradeDto } from '../../../core/services/trade.service';
import { ToyService } from '../../../core/services/toy.service';

@Component({
  selector: 'app-trade-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './trade-success.component.html',
  styleUrl: './trade-success.component.css'
})
export class TradeSuccessComponent implements OnInit {
  trade = signal<TradeDto | null>(null);
  loading = signal(true);
  error = signal('');

  constructor(
    private route: ActivatedRoute,
    private tradeService: TradeService,
    public toyService: ToyService
  ) {}

  ngOnInit(): void {
    const tradeId = this.route.snapshot.paramMap.get('id');
    if (tradeId) {
      this.loadTrade(tradeId);
    } else {
      this.loading.set(false);
      this.error.set('Trade not found.');
    }
  }

  private loadTrade(id: string): void {
    this.tradeService.getTradeById(id).subscribe({
      next: (trade) => {
        this.trade.set(trade);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load trade details.');
        this.loading.set(false);
      }
    });
  }
}
