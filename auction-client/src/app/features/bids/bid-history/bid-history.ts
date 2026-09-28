import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe, AsyncPipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../../core/services/auction';

interface UserBid {
  bidId: string;
  auctionId: string;
  auctionTitle: string;
  amount: number;
  placedAt: string;
}

@Component({
  selector: 'app-bid-history',
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe, AsyncPipe],
  template: `
    <div class="container">
      <h1>My Bid History</h1>
      <div class="bid-history-list">
        @for (bid of bids$ | async; track bid.bidId) {
          <div class="bid-history-item">
            <a [routerLink]="['/auctions', bid.auctionId]" class="auction-link">{{ bid.auctionTitle }}</a>
            <span class="bid-amount">{{ bid.amount | currency }}</span>
            <span class="bid-date">{{ bid.placedAt | date:'medium' }}</span>
          </div>
        } @empty {
          <div class="empty">No bids placed yet.</div>
        }
      </div>

      @if (totalPages > 1) {
        <div class="pagination">
          <button (click)="loadPage(page - 1)" [disabled]="!hasPrev">Previous</button>
          <span>Page {{ page }} of {{ totalPages }}</span>
          <button (click)="loadPage(page + 1)" [disabled]="!hasNext">Next</button>
        </div>
      }
    </div>
  `
})
export class BidHistoryComponent implements OnInit {
  private http = inject(HttpClient);
  private bidsSubject = new BehaviorSubject<UserBid[]>([]);
  bids$ = this.bidsSubject.asObservable();

  page = 1;
  readonly pageSize = 20;
  totalPages = 1;
  hasPrev = false;
  hasNext = false;

  ngOnInit() {
    this.loadPage(1);
  }

  loadPage(page: number): void {
    this.http
      .get<PaginatedResult<UserBid>>(
        `${environment.apiUrl}/bids/history?page=${page}&pageSize=${this.pageSize}`)
      .subscribe({
        next: r => {
          this.bidsSubject.next(r.items);
          this.page       = r.page;
          this.totalPages = r.totalPages;
          this.hasPrev    = r.hasPreviousPage;
          this.hasNext    = r.hasNextPage;
        },
        error: err => console.error('Failed to load bid history', err)
      });
  }
}
