import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from './toy.service';

export interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  totalToys: number;
  availableToys: number;
  pendingTrades: number;
  completedTrades: number;
  pendingReturns: number;
  totalRevenue: number;
  recentActivities: RecentActivity[];
}

export interface RecentActivity {
  type: string;
  description: string;
  timestamp: string;
}

export interface AdminUserDto {
  id: string;
  email: string;
  fullName: string;
  role: string;
  profileImageUrl: string | null;
  reputationScore: number;
  totalTradesCompleted: number;
  isActive: boolean;
  createdAt: string;
}

export interface UserQueryParams {
  searchTerm?: string;
  isActive?: boolean;
  role?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly apiUrl = `${environment.apiUrl}/admin`;

  loading = signal<boolean>(false);

  constructor(private http: HttpClient) {}

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard`);
  }

  getUsers(params: UserQueryParams = {}): Observable<PagedResult<AdminUserDto>> {
    let httpParams = new HttpParams();

    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    if (params.role) httpParams = httpParams.set('role', params.role);
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResult<AdminUserDto>>(`${this.apiUrl}/users`, { params: httpParams });
  }

  activateUser(userId: string): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.apiUrl}/users/${userId}/activate`, {});
  }

  deactivateUser(userId: string): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.apiUrl}/users/${userId}/deactivate`, {});
  }

  getActivityTypeIcon(type: string): string {
    switch (type) {
      case 'Trade': return 'M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4';
      case 'Purchase': return 'M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 100 4 2 2 0 000-4z';
      case 'Return': return 'M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6';
      case 'ReturnApproved': return 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z';
      case 'ReturnRejected': return 'M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z';
      default: return 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z';
    }
  }

  getActivityTypeBadgeClass(type: string): string {
    switch (type) {
      case 'Trade': return 'bg-blue-100 text-blue-800';
      case 'Purchase': return 'bg-green-100 text-green-800';
      case 'Return': return 'bg-yellow-100 text-yellow-800';
      case 'ReturnApproved': return 'bg-emerald-100 text-emerald-800';
      case 'ReturnRejected': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
