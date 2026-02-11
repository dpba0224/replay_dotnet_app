import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TradeService, TradeDto, TradeQueryParams } from '../../../core/services/trade.service';
import { ReturnService, ReturnDto, ReturnQueryParams, ApproveReturnDto } from '../../../core/services/return.service';
import { ToyService } from '../../../core/services/toy.service';
import { PagedResult } from '../../../core/services/toy.service';
import { ToyConditions } from '../../../core/services/toy.service';
import { StarRatingComponent } from '../../../shared/components/star-rating/star-rating.component';

@Component({
  selector: 'app-trade-monitor',
  standalone: true,
  imports: [CommonModule, FormsModule, StarRatingComponent],
  templateUrl: './trade-monitor.component.html',
  styleUrl: './trade-monitor.component.css'
})
export class TradeMonitorComponent implements OnInit {
  activeTab = signal<'trades' | 'returns'>('trades');

  // Trades
  trades = signal<PagedResult<TradeDto> | null>(null);
  tradesLoading = signal(true);
  tradesError = signal<string | null>(null);
  tradeFilters: TradeQueryParams = { pageNumber: 1, pageSize: 10 };

  // Returns
  returns = signal<PagedResult<ReturnDto> | null>(null);
  returnsLoading = signal(true);
  returnsError = signal<string | null>(null);
  returnFilters: ReturnQueryParams = { pageNumber: 1, pageSize: 10 };

  // Approve return modal
  showApproveModal = signal(false);
  selectedReturn = signal<ReturnDto | null>(null);
  approveForm: ApproveReturnDto = { conditionOnReturn: 3 };
  approveSubmitting = signal(false);
  approveError = signal<string | null>(null);

  // Reject return modal
  showRejectModal = signal(false);
  rejectNotes = '';
  rejectSubmitting = signal(false);
  rejectError = signal<string | null>(null);

  conditions = ToyConditions;

  constructor(
    public tradeService: TradeService,
    public returnService: ReturnService,
    public toyService: ToyService
  ) {}

  ngOnInit(): void {
    this.loadTrades();
    this.loadReturns();
  }

  switchTab(tab: 'trades' | 'returns'): void {
    this.activeTab.set(tab);
  }

  // --- Trades ---

  loadTrades(): void {
    this.tradesLoading.set(true);
    this.tradesError.set(null);
    this.tradeService.getAllTrades(this.tradeFilters).subscribe({
      next: (result) => {
        this.trades.set(result);
        this.tradesLoading.set(false);
      },
      error: (err) => {
        this.tradesError.set(err.error?.message || 'Failed to load trades.');
        this.tradesLoading.set(false);
      }
    });
  }

  onTradeFilterChange(): void {
    this.tradeFilters.pageNumber = 1;
    this.loadTrades();
  }

  goToTradePage(page: number): void {
    this.tradeFilters.pageNumber = page;
    this.loadTrades();
  }

  approveTrade(trade: TradeDto): void {
    this.tradeService.approveTrade(trade.id).subscribe({
      next: (result) => {
        if (!result.succeeded) {
          this.tradesError.set(result.message || 'Failed to approve trade.');
        }
        this.loadTrades();
      },
      error: (err) => {
        this.tradesError.set(err.error?.message || 'Failed to approve trade.');
      }
    });
  }

  cancelTrade(trade: TradeDto): void {
    this.tradeService.cancelTrade(trade.id).subscribe({
      next: (result) => {
        if (!result.succeeded) {
          this.tradesError.set(result.message || 'Failed to cancel trade.');
        }
        this.loadTrades();
      },
      error: (err) => {
        this.tradesError.set(err.error?.message || 'Failed to cancel trade.');
      }
    });
  }

  // --- Returns ---

  loadReturns(): void {
    this.returnsLoading.set(true);
    this.returnsError.set(null);
    this.returnService.getAllReturns(this.returnFilters).subscribe({
      next: (result) => {
        this.returns.set(result);
        this.returnsLoading.set(false);
      },
      error: (err) => {
        this.returnsError.set(err.error?.message || 'Failed to load returns.');
        this.returnsLoading.set(false);
      }
    });
  }

  onReturnFilterChange(): void {
    this.returnFilters.pageNumber = 1;
    this.loadReturns();
  }

  goToReturnPage(page: number): void {
    this.returnFilters.pageNumber = page;
    this.loadReturns();
  }

  openApproveModal(ret: ReturnDto): void {
    this.selectedReturn.set(ret);
    this.approveForm = {
      conditionOnReturn: ret.toy.condition,
      adminNotes: '',
      userRating: undefined,
      ratingComment: ''
    };
    this.approveError.set(null);
    this.showApproveModal.set(true);
  }

  closeApproveModal(): void {
    this.showApproveModal.set(false);
    this.selectedReturn.set(null);
  }

  submitApproval(): void {
    const ret = this.selectedReturn();
    if (!ret) return;

    this.approveSubmitting.set(true);
    this.approveError.set(null);

    this.returnService.approveReturn(ret.id, this.approveForm).subscribe({
      next: (result) => {
        this.approveSubmitting.set(false);
        if (result.succeeded) {
          this.closeApproveModal();
          this.loadReturns();
        } else {
          this.approveError.set(result.message || 'Failed to approve return.');
        }
      },
      error: (err) => {
        this.approveSubmitting.set(false);
        this.approveError.set(err.error?.message || 'Failed to approve return.');
      }
    });
  }

  openRejectModal(ret: ReturnDto): void {
    this.selectedReturn.set(ret);
    this.rejectNotes = '';
    this.rejectError.set(null);
    this.showRejectModal.set(true);
  }

  closeRejectModal(): void {
    this.showRejectModal.set(false);
    this.selectedReturn.set(null);
  }

  submitRejection(): void {
    const ret = this.selectedReturn();
    if (!ret || !this.rejectNotes.trim()) return;

    this.rejectSubmitting.set(true);
    this.rejectError.set(null);

    this.returnService.rejectReturn(ret.id, this.rejectNotes).subscribe({
      next: (result) => {
        this.rejectSubmitting.set(false);
        if (result.succeeded) {
          this.closeRejectModal();
          this.loadReturns();
        } else {
          this.rejectError.set(result.message || 'Failed to reject return.');
        }
      },
      error: (err) => {
        this.rejectSubmitting.set(false);
        this.rejectError.set(err.error?.message || 'Failed to reject return.');
      }
    });
  }

  getPendingReturnCount(): number {
    return this.returns()?.items.filter(r => r.status === 'Pending').length ?? 0;
  }
}
