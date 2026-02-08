import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface MessageDto {
  id: string;
  senderId: string;
  senderName: string;
  receiverId: string;
  receiverName: string;
  tradeId: string | null;
  content: string;
  isRead: boolean;
  createdAt: string;
}

export interface ConversationDto {
  userId: string;
  userName: string;
  userProfileImage: string | null;
  lastMessage: string;
  lastMessageAt: string;
  unreadCount: number;
}

export interface SendMessageDto {
  receiverId: string;
  tradeId?: string;
  content: string;
}

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private apiUrl = `${environment.apiUrl}/messages`;

  constructor(private http: HttpClient) {}

  sendMessage(dto: SendMessageDto): Observable<MessageDto> {
    return this.http.post<MessageDto>(this.apiUrl, dto);
  }

  getConversations(): Observable<ConversationDto[]> {
    return this.http.get<ConversationDto[]>(`${this.apiUrl}/conversations`);
  }

  getConversationMessages(userId: string): Observable<MessageDto[]> {
    return this.http.get<MessageDto[]>(`${this.apiUrl}/conversations/${userId}`);
  }

  markAsRead(messageId: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${messageId}/read`, {});
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/unread-count`);
  }
}
