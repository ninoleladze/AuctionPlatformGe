import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe, AsyncPipe } from '@angular/common';
import { Store } from '@ngrx/store';
import {
  selectAllAuctions,
  selectAuctionsLoading,
  selectCurrentPage,
  selectTotalPages
} from '../../../store/auctions/auctions.selectors';
import * as AuctionsActions from '../../../store/auctions/auctions.actions';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-auction-list',
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe, AsyncPipe],
  template: `
    <div class="container">
      <h1>Live Auctions</h1>
      @if (loading$ | async) {
        <div class="loading">Loading auctions...</div>
      }
      <div class="auctions-grid">
        @for (auction of auctions$ | async; track auction.id) {
          <div class="auction-card" [routerLink]="['/auctions', auction.id]">
            @if (auction.imageUrl) {
              <img class="auction-thumb" [src]="auction.imageUrl" [alt]="auction.title" />
            }
            <div class="auction-type">{{ auction.itemType }}</div>
            <h2>{{ auction.title }}</h2>
            <p>{{ auction.description }}</p>
            <div class="auction-info">
              <div class="bid-info">
                <span class="label">Current Bid</span>
                <span class="amount">{{ auction.currentBid | currency }}</span>
              </div>
              <div class="bid-info">
                <span class="label">Bids</span>
                <span class="amount">{{ auction.bidCount }}</span>
              </div>
            </div>
            <div class="ends-at">Ends: {{ auction.endsAt | date:'medium' }}</div>
            <div class="status" [class.active]="auction.status === 'Active'">{{ auction.status }}</div>
          </div>
        } @empty {
          @if (!(loading$ | async)) {
            <div class="empty">
              No auctions yet. <a routerLink="/auctions/create">Create one!</a>
            </div>
          }
        }
      </div>

      <!-- Pagination -->
      @if (totalPages() > 1) {
        <div class="pagination">
          <button [disabled]="currentPage() <= 1" (click)="changePage(currentPage() - 1)">
            &laquo; Prev
          </button>
          <span>Page {{ currentPage() }} of {{ totalPages() }}</span>
          <button [disabled]="currentPage() >= totalPages()" (click)="changePage(currentPage() + 1)">
            Next &raquo;
          </button>
        </div>
      }
    </div>
  `
})
export class AuctionListComponent implements OnInit {
  private store = inject(Store);
  auctions$ = this.store.select(selectAllAuctions);
  loading$ = this.store.select(selectAuctionsLoading);
  currentPage = toSignal(this.store.select(selectCurrentPage), { initialValue: 1 });
  totalPages = toSignal(this.store.select(selectTotalPages), { initialValue: 1 });

  ngOnInit() {
    this.store.dispatch(AuctionsActions.loadAuctions({ page: 1 }));
  }

  changePage(page: number) {
    this.store.dispatch(AuctionsActions.loadAuctions({ page }));
  }
}
