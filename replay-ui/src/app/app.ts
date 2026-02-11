import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { RouterOutlet, RouterLink, NavigationEnd, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription, filter } from 'rxjs';
import { AuthService } from './core/services/auth.service';
import { MessageService } from './core/services/message.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit, OnDestroy {
  unreadCount = signal(0);
  mobileMenuOpen = signal(false);
  private pollInterval: any;
  private routerSub?: Subscription;

  constructor(
    public authService: AuthService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Load unread count on navigation and poll periodically
    this.routerSub = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.mobileMenuOpen.set(false);
      if (this.authService.isAuthenticated()) {
        this.loadUnreadCount();
      }
    });

    if (this.authService.isAuthenticated()) {
      this.loadUnreadCount();
    }

    this.pollInterval = setInterval(() => {
      if (this.authService.isAuthenticated()) {
        this.loadUnreadCount();
      }
    }, 30000);
  }

  ngOnDestroy(): void {
    if (this.pollInterval) clearInterval(this.pollInterval);
    this.routerSub?.unsubscribe();
  }

  logout(): void {
    this.authService.logout();
    this.unreadCount.set(0);
  }

  private loadUnreadCount(): void {
    this.messageService.getUnreadCount().subscribe({
      next: (result) => this.unreadCount.set(result.count),
      error: () => {}
    });
  }
}
