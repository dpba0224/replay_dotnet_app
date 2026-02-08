import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PaymentResult {
  succeeded: boolean;
  message: string | null;
  checkoutUrl: string | null;
  paymentIntentId: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private readonly apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  createCheckoutSession(tradeId: string): Observable<PaymentResult> {
    return this.http.post<PaymentResult>(`${this.apiUrl}/create-checkout`, { tradeId });
  }

  redirectToCheckout(checkoutUrl: string): void {
    window.location.href = checkoutUrl;
  }
}
