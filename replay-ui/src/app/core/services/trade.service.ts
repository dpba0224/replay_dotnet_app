import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Toy, PagedResult } from './toy.service';

export interface TradeDto {
  id: string;
  requestedToy: Toy;
  offeredToy: Toy | null;
  user: TradeUser;
  tradeType: string;
  status: string;
  amountPaid: number | null;
  notes: string | null;
  createdAt: string;
  completedAt: string | null;
}

export interface TradeUser {
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

export interface CreateTradeDto {
  requestedToyId: string;
  offeredToyId?: string;
  tradeType: number; // 0 = Trade, 1 = Purchase
  notes?: string;
}

export interface TradeResult {
  succeeded: boolean;
  message: string | null;
  trade: TradeDto | null;
  stripeCheckoutUrl: string | null;
}

export interface TradeQueryParams {
  status?: string;
  type?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class TradeService {
  private readonly apiUrl = `${environment.apiUrl}/trades`;

  loading = signal<boolean>(false);

  constructor(private http: HttpClient) {}

  createTrade(dto: CreateTradeDto): Observable<TradeResult> {
    return this.http.post<TradeResult>(this.apiUrl, dto);
  }

  getUserTrades(params: TradeQueryParams = {}): Observable<PagedResult<TradeDto>> {
    let httpParams = new HttpParams();

    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.type) httpParams = httpParams.set('type', params.type);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResult<TradeDto>>(this.apiUrl, { params: httpParams });
  }

  getAllTrades(params: TradeQueryParams = {}): Observable<PagedResult<TradeDto>> {
    let httpParams = new HttpParams();

    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.type) httpParams = httpParams.set('type', params.type);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResult<TradeDto>>(`${this.apiUrl}/all`, { params: httpParams });
  }

  getTradeById(id: string): Observable<TradeDto> {
    return this.http.get<TradeDto>(`${this.apiUrl}/${id}`);
  }

  approveTrade(id: string): Observable<TradeResult> {
    return this.http.patch<TradeResult>(`${this.apiUrl}/${id}/approve`, {});
  }

  cancelTrade(id: string): Observable<TradeResult> {
    return this.http.patch<TradeResult>(`${this.apiUrl}/${id}/cancel`, {});
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Pending': return 'bg-yellow-100 text-yellow-800';
      case 'Approved': return 'bg-green-100 text-green-800';
      case 'Completed': return 'bg-blue-100 text-blue-800';
      case 'Rejected': return 'bg-red-100 text-red-800';
      case 'Cancelled': return 'bg-gray-100 text-gray-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  getTradeTypeLabel(type: string): string {
    switch (type) {
      case 'Trade': return 'Toy Trade';
      case 'Purchase': return 'Purchase';
      default: return type;
    }
  }
}
