import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransactionService, TransactionDto, TransactionQueryParams } from '../../../core/services/transaction.service';
import { ToyService, PagedResult } from '../../../core/services/toy.service';

@Component({
  selector: 'app-transaction-log',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transaction-log.html',
  styleUrl: './transaction-log.css'
})
export class TransactionLogComponent implements OnInit {
  transactions = signal<PagedResult<TransactionDto> | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  filters: TransactionQueryParams = {
    pageNumber: 1,
    pageSize: 15
  };

  constructor(
    public transactionService: TransactionService,
    public toyService: ToyService
  ) {}

  ngOnInit(): void {
    this.loadTransactions();
  }

  loadTransactions(): void {
    this.loading.set(true);
    this.error.set(null);

    this.transactionService.getAllTransactions(this.filters).subscribe({
      next: (result) => {
        this.transactions.set(result);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load transactions.');
        this.loading.set(false);
      }
    });
  }

  onFilterChange(): void {
    this.filters.pageNumber = 1;
    this.loadTransactions();
  }

  clearFilters(): void {
    this.filters = { pageNumber: 1, pageSize: 15 };
    this.loadTransactions();
  }

  goToPage(page: number): void {
    this.filters.pageNumber = page;
    this.loadTransactions();
  }

  get hasActiveFilters(): boolean {
    return !!(this.filters.type || this.filters.fromDate || this.filters.toDate);
  }

  getTotalRevenue(): number {
    const items = this.transactions()?.items;
    if (!items) return 0;
    return items
      .filter(t => t.amountPaid != null)
      .reduce((sum, t) => sum + (t.amountPaid ?? 0), 0);
  }
}
