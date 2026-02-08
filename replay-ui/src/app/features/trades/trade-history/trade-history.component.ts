import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TradeService, TradeDto, TradeQueryParams } from '../../../core/services/trade.service';
import { ToyService, PagedResult } from '../../../core/services/toy.service';

@Component({
  selector: 'app-trade-history',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './trade-history.component.html',
  styleUrl: './trade-history.component.css'
})
export class TradeHistoryComponent implements OnInit {
  trades = signal<TradeDto[]>([]);
  loading = signal(true);
  error = signal('');

  // Pagination
  pageNumber = signal(1);
  pageSize = 10;
  totalCount = signal(0);
  totalPages = signal(0);

  // Filters
  statusFilter = '';
  typeFilter = '';

  constructor(
    public tradeService: TradeService,
    public toyService: ToyService
  ) {}

  ngOnInit(): void {
    this.loadTrades();
  }

  loadTrades(): void {
    this.loading.set(true);
    this.error.set('');

    const params: TradeQueryParams = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize
    };

    if (this.statusFilter) params.status = this.statusFilter;
    if (this.typeFilter) params.type = this.typeFilter;

    this.tradeService.getUserTrades(params).subscribe({
      next: (result: PagedResult<TradeDto>) => {
        this.trades.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load trades.');
        this.loading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.pageNumber.set(1);
    this.loadTrades();
  }

  clearFilters(): void {
    this.statusFilter = '';
    this.typeFilter = '';
    this.pageNumber.set(1);
    this.loadTrades();
  }

  goToPage(page: number): void {
    this.pageNumber.set(page);
    this.loadTrades();
  }

  cancelTrade(tradeId: string): void {
    this.tradeService.cancelTrade(tradeId).subscribe({
      next: (result) => {
        if (result.succeeded) {
          this.loadTrades();
        }
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to cancel trade.');
      }
    });
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
