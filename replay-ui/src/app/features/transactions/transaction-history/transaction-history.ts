import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TransactionService, TransactionDto, TransactionQueryParams } from '../../../core/services/transaction.service';
import { ToyService, PagedResult } from '../../../core/services/toy.service';

@Component({
  selector: 'app-transaction-history',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './transaction-history.html',
  styleUrl: './transaction-history.css'
})
export class TransactionHistoryComponent implements OnInit {
  transactions = signal<TransactionDto[]>([]);
  loading = signal(true);
  error = signal('');

  // Pagination
  pageNumber = signal(1);
  pageSize = 10;
  totalCount = signal(0);
  totalPages = signal(0);

  // Filters
  typeFilter = '';
  fromDate = '';
  toDate = '';

  constructor(
    public transactionService: TransactionService,
    public toyService: ToyService
  ) {}

  ngOnInit(): void {
    this.loadTransactions();
  }

  loadTransactions(): void {
    this.loading.set(true);
    this.error.set('');

    const params: TransactionQueryParams = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize
    };

    if (this.typeFilter) params.type = this.typeFilter;
    if (this.fromDate) params.fromDate = this.fromDate;
    if (this.toDate) params.toDate = this.toDate;

    this.transactionService.getUserTransactions(params).subscribe({
      next: (result: PagedResult<TransactionDto>) => {
        this.transactions.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load transaction history.');
        this.loading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.pageNumber.set(1);
    this.loadTransactions();
  }

  clearFilters(): void {
    this.typeFilter = '';
    this.fromDate = '';
    this.toDate = '';
    this.pageNumber.set(1);
    this.loadTransactions();
  }

  goToPage(page: number): void {
    this.pageNumber.set(page);
    this.loadTransactions();
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  get hasActiveFilters(): boolean {
    return !!(this.typeFilter || this.fromDate || this.toDate);
  }
}
