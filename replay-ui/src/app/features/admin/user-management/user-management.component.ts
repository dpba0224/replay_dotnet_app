import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, AdminUserDto, UserQueryParams } from '../../../core/services/admin.service';
import { PagedResult } from '../../../core/services/toy.service';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  users = signal<PagedResult<AdminUserDto> | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  actionMessage = signal<string | null>(null);

  filters: UserQueryParams = {
    pageNumber: 1,
    pageSize: 15
  };
  searchTerm = '';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.error.set(null);

    if (this.searchTerm.trim()) {
      this.filters.searchTerm = this.searchTerm.trim();
    } else {
      this.filters.searchTerm = undefined;
    }

    this.adminService.getUsers(this.filters).subscribe({
      next: (result) => {
        this.users.set(result);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load users.');
        this.loading.set(false);
      }
    });
  }

  onSearch(): void {
    this.filters.pageNumber = 1;
    this.loadUsers();
  }

  onFilterChange(): void {
    this.filters.pageNumber = 1;
    this.loadUsers();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.filters = { pageNumber: 1, pageSize: 15 };
    this.loadUsers();
  }

  goToPage(page: number): void {
    this.filters.pageNumber = page;
    this.loadUsers();
  }

  activateUser(user: AdminUserDto): void {
    this.actionMessage.set(null);
    this.adminService.activateUser(user.id).subscribe({
      next: () => {
        this.actionMessage.set(`${user.fullName} has been activated.`);
        this.loadUsers();
        this.clearActionMessageAfterDelay();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to activate user.');
      }
    });
  }

  deactivateUser(user: AdminUserDto): void {
    this.actionMessage.set(null);
    this.adminService.deactivateUser(user.id).subscribe({
      next: () => {
        this.actionMessage.set(`${user.fullName} has been deactivated.`);
        this.loadUsers();
        this.clearActionMessageAfterDelay();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to deactivate user.');
      }
    });
  }

  get hasActiveFilters(): boolean {
    return !!(this.searchTerm || this.filters.isActive !== undefined);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  private clearActionMessageAfterDelay(): void {
    setTimeout(() => this.actionMessage.set(null), 3000);
  }
}
