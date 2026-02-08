import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Toy, PagedResult } from './toy.service';

export interface ReturnUser {
  id: string;
  email: string;
  fullName: string;
  reputationScore: number;
  totalTradesCompleted: number;
  isActive: boolean;
  createdAt: string;
}

export interface ReturnDto {
  id: string;
  toy: Toy;
  returnedByUser: ReturnUser;
  approvedByAdmin: ReturnUser | null;
  status: string;
  conditionOnReturn: number | null;
  userNotes: string | null;
  adminNotes: string | null;
  createdAt: string;
  resolvedAt: string | null;
}

export interface CreateReturnDto {
  toyId: string;
  userNotes?: string;
}

export interface ApproveReturnDto {
  conditionOnReturn: number;
  adminNotes?: string;
  userRating?: number;
  ratingComment?: string;
}

export interface ReturnResult {
  succeeded: boolean;
  message: string | null;
  return: ReturnDto | null;
}

export interface ReturnQueryParams {
  status?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ReturnService {
  private readonly apiUrl = `${environment.apiUrl}/returns`;

  constructor(private http: HttpClient) {}

  initiateReturn(dto: CreateReturnDto): Observable<ReturnResult> {
    return this.http.post<ReturnResult>(this.apiUrl, dto);
  }

  getUserReturns(params: ReturnQueryParams = {}): Observable<PagedResult<ReturnDto>> {
    let httpParams = new HttpParams();

    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResult<ReturnDto>>(this.apiUrl, { params: httpParams });
  }

  getAllReturns(params: ReturnQueryParams = {}): Observable<PagedResult<ReturnDto>> {
    let httpParams = new HttpParams();

    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResult<ReturnDto>>(`${this.apiUrl}/all`, { params: httpParams });
  }

  approveReturn(id: string, dto: ApproveReturnDto): Observable<ReturnResult> {
    return this.http.patch<ReturnResult>(`${this.apiUrl}/${id}/approve`, dto);
  }

  rejectReturn(id: string, adminNotes: string): Observable<ReturnResult> {
    return this.http.patch<ReturnResult>(`${this.apiUrl}/${id}/reject`, { adminNotes });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Pending': return 'bg-yellow-100 text-yellow-800';
      case 'Approved': return 'bg-green-100 text-green-800';
      case 'Rejected': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
