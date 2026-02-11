import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface RatingDto {
  id: string;
  ratedUserId: string;
  ratedUserName: string;
  ratedByAdminId: string;
  ratedByAdminName: string;
  toyReturnId: string;
  score: number;
  comment: string | null;
  createdAt: string;
}

export interface ReputationResponse {
  reputationScore: number;
}

@Injectable({
  providedIn: 'root'
})
export class RatingService {
  private readonly apiUrl = `${environment.apiUrl}/ratings`;

  constructor(private http: HttpClient) {}

  getUserRatings(userId: string): Observable<RatingDto[]> {
    return this.http.get<RatingDto[]>(`${this.apiUrl}/user/${userId}`);
  }

  getUserReputation(userId: string): Observable<ReputationResponse> {
    return this.http.get<ReputationResponse>(`${this.apiUrl}/user/${userId}/reputation`);
  }
}
