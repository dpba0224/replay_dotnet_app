import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/toys/toy-list/toy-list.component').then(m => m.ToyListComponent)
  },
  {
    path: 'auth',
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
      },
      {
        path: 'verify-email',
        loadComponent: () => import('./features/auth/email-verify/email-verify.component').then(m => m.EmailVerifyComponent)
      },
      {
        path: 'forgot-password',
        loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
      },
      {
        path: 'reset-password',
        loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
      }
    ]
  },
  {
    path: 'toys',
    children: [
      {
        path: ':id',
        loadComponent: () => import('./features/toys/toy-detail/toy-detail.component').then(m => m.ToyDetailComponent)
      }
    ]
  },
  {
    path: 'my-toys',
    canActivate: [authGuard],
    loadComponent: () => import('./features/toys/my-toys/my-toys.component').then(m => m.MyToysComponent)
  },
  {
    path: 'transactions',
    canActivate: [authGuard],
    loadComponent: () => import('./features/transactions/transaction-history/transaction-history').then(m => m.TransactionHistoryComponent)
  },
  {
    path: 'trades',
    canActivate: [authGuard],
    children: [
      {
        path: 'history',
        loadComponent: () => import('./features/trades/trade-history/trade-history.component').then(m => m.TradeHistoryComponent)
      },
      {
        path: ':id/success',
        loadComponent: () => import('./features/trades/trade-success/trade-success.component').then(m => m.TradeSuccessComponent)
      },
      {
        path: ':id',
        loadComponent: () => import('./features/trades/trade-detail/trade-detail.component').then(m => m.TradeDetailComponent)
      }
    ]
  },
  {
    path: 'messages',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/messages/messages.component').then(m => m.MessagesComponent)
      },
      {
        path: ':userId',
        loadComponent: () => import('./features/messages/conversation/conversation.component').then(m => m.ConversationComponent)
      }
    ]
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/admin/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./features/admin/user-management/user-management.component').then(m => m.UserManagementComponent)
      },
      {
        path: 'toys',
        loadComponent: () => import('./features/admin/toy-inventory/toy-inventory.component').then(m => m.ToyInventoryComponent)
      },
      {
        path: 'toys/new',
        loadComponent: () => import('./features/admin/toy-form/toy-form.component').then(m => m.ToyFormComponent)
      },
      {
        path: 'toys/:id/edit',
        loadComponent: () => import('./features/admin/toy-form/toy-form.component').then(m => m.ToyFormComponent)
      },
      {
        path: 'trades',
        loadComponent: () => import('./features/admin/trade-monitor/trade-monitor.component').then(m => m.TradeMonitorComponent)
      },
      {
        path: 'transactions',
        loadComponent: () => import('./features/admin/transaction-log/transaction-log').then(m => m.TransactionLogComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
