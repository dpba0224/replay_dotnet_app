import { Component, OnInit, OnDestroy, signal, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MessageService, MessageDto } from '../../../core/services/message.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-conversation',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './conversation.component.html',
  styleUrl: './conversation.component.css'
})
export class ConversationComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef;

  otherUserId = '';
  otherUserName = signal('');
  messages = signal<MessageDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  newMessage = '';
  sending = signal(false);
  private shouldScroll = false;
  private pollInterval: any;

  constructor(
    private route: ActivatedRoute,
    private messageService: MessageService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.otherUserId = this.route.snapshot.paramMap.get('userId') || '';
    if (this.otherUserId) {
      this.loadMessages();
      // Poll for new messages every 10 seconds
      this.pollInterval = setInterval(() => this.loadMessages(false), 10000);
    }
  }

  ngOnDestroy(): void {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
    }
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  loadMessages(showLoading = true): void {
    if (showLoading) this.loading.set(true);
    this.error.set(null);

    this.messageService.getConversationMessages(this.otherUserId).subscribe({
      next: (messages) => {
        const prevCount = this.messages().length;
        this.messages.set(messages);
        if (showLoading || messages.length > prevCount) {
          this.shouldScroll = true;
        }
        // Set other user's name from messages
        if (messages.length > 0) {
          const currentUserId = this.authService.currentUser()?.id;
          const otherMsg = messages.find(m => m.senderId !== currentUserId);
          const ownMsg = messages.find(m => m.senderId === currentUserId);
          if (otherMsg) {
            this.otherUserName.set(otherMsg.senderName);
          } else if (ownMsg) {
            this.otherUserName.set(ownMsg.receiverName);
          }
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load messages.');
        this.loading.set(false);
      }
    });
  }

  sendMessage(): void {
    const content = this.newMessage.trim();
    if (!content || this.sending()) return;

    this.sending.set(true);
    this.messageService.sendMessage({
      receiverId: this.otherUserId,
      content
    }).subscribe({
      next: (message) => {
        this.messages.update(msgs => [...msgs, message]);
        this.newMessage = '';
        this.sending.set(false);
        this.shouldScroll = true;
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to send message.');
        this.sending.set(false);
      }
    });
  }

  isOwnMessage(message: MessageDto): boolean {
    return message.senderId === this.authService.currentUser()?.id;
  }

  formatTime(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  formatDateHeader(dateStr: string): string {
    const date = new Date(dateStr);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    if (date.toDateString() === today.toDateString()) return 'Today';
    if (date.toDateString() === yesterday.toDateString()) return 'Yesterday';
    return date.toLocaleDateString(undefined, { weekday: 'long', month: 'short', day: 'numeric' });
  }

  shouldShowDateHeader(index: number): boolean {
    if (index === 0) return true;
    const current = new Date(this.messages()[index].createdAt).toDateString();
    const prev = new Date(this.messages()[index - 1].createdAt).toDateString();
    return current !== prev;
  }

  private scrollToBottom(): void {
    try {
      if (this.messagesContainer) {
        this.messagesContainer.nativeElement.scrollTop = this.messagesContainer.nativeElement.scrollHeight;
      }
    } catch (err) {}
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }
}
