import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auctions', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
  },
  {
    path: 'auctions',
    loadComponent: () => import('./features/auctions/auction-list/auction-list').then(m => m.AuctionListComponent)
  },
  {
    path: 'auctions/create',
    canActivate: [authGuard],
    loadComponent: () => import('./features/auctions/create-auction/create-auction').then(m => m.CreateAuction)
  },
  {
    path: 'auctions/:id',
    loadComponent: () => import('./features/auctions/auction-detail/auction-detail').then(m => m.AuctionDetailComponent)
  },
  {
    path: 'my-auctions',
    canActivate: [authGuard],
    loadComponent: () => import('./features/auctions/my-auctions/my-auctions').then(m => m.MyAuctionsComponent)
  },
  {
    path: 'bids/history',
    canActivate: [authGuard],
    loadComponent: () => import('./features/bids/bid-history/bid-history').then(m => m.BidHistoryComponent)
  },
  {
    path: 'wallet',
    canActivate: [authGuard],
    loadComponent: () => import('./features/wallet/wallet').then(m => m.WalletComponent)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/admin-panel').then(m => m.AdminPanelComponent)
  }
];
