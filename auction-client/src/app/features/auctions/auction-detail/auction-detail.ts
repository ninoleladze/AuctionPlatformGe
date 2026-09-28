import { Component, OnInit, OnDestroy, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe, SlicePipe, CurrencyPipe, AsyncPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { take, withLatestFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuctionHubService } from '../../../core/hubs/auction-hub';
import { selectSelectedAuction } from '../../../store/auctions/auctions.selectors';
import { selectIsLoggedIn, selectUserId } from '../../../store/auth/auth.selectors';
import * as AuctionsActions from '../../../store/auctions/auctions.actions';

@Component({
  selector: 'app-auction-detail',
  standalone: true,
  imports: [DatePipe, SlicePipe, CurrencyPipe, FormsModule, RouterLink, AsyncPipe],
  template: `
    @if (auction$ | async; as auction) {
      <div class="container">
        <div class="auction-detail">
          <div class="auction-header">
            @if (auction.imageUrl) {
              <img class="auction-image" [src]="auction.imageUrl" [alt]="auction.title" />
            }
            <span class="item-type">{{ auction.itemType }}</span>
            <h1>{{ auction.title }}</h1>
            <p class="description">{{ auction.description }}</p>
          </div>

          <div class="bid-panel">
            <div class="current-bid" [class.pulse]="justBid">
              <span class="label">Current Bid</span>
              <span class="amount">{{ auction.currentBid | currency }}</span>
            </div>
            <div class="bid-stats">
              <span>{{ auction.bidCount }} bids</span>
              @if (auction.status !== 'Ended') {
                <span>Ends: {{ auction.endsAt | date:'medium' }}</span>
              }
            </div>

            @if (auction.status === 'Ended') {
              <div class="auction-ended-banner">
                @if (reserveNotMet) {
                  Reserve price not met — auction ended without a winner.
                } @else {
                  Auction Ended — Final Bid: {{ auction.currentBid | currency }}
                  @if (auction.highestBidderId) {
                    <span> · Winner: {{ auction.highestBidderId | slice:0:8 }}...</span>
                  }
                }
              </div>
            }

            @if (extended) {
              <div class="bid-extended">Extended by 10 seconds — last-minute bid!</div>
            }
            @if (isHighBidder && auction.status === 'Active') {
              <div class="high-bidder-badge">You are the highest bidder!</div>
            }
            @if (bidError) {
              <div class="bid-error">{{ bidError }}</div>
            }
            @if (bidSuccess) {
              <div class="bid-success">Bid placed successfully!</div>
            }

            @if ((isLoggedIn$ | async) && auction.status === 'Active') {
              <div class="place-bid">
                <input
                  type="number"
                  [(ngModel)]="bidAmount"
                  [placeholder]="'Enter bid above ' + auction.currentBid"
                />
                <button (click)="placeBid(auction.id, auction.currentBid)" [disabled]="placing">
                  {{ placing ? 'Placing...' : 'Place Bid' }}
                </button>
              </div>

              <!-- Auto-bid section -->
              <div class="auto-bid-section">
                <h4>Auto-Bid (proxy bidding)</h4>
                @if (autoBidMax !== null) {
                  <div class="auto-bid-active">
                    Active auto-bid: max {{ autoBidMax | currency }}
                    <button class="cancel-btn" (click)="cancelAutoBid(auction.id)" [disabled]="autoBidLoading">
                      Cancel
                    </button>
                  </div>
                } @else {
                  <div class="auto-bid-form">
                    <input
                      type="number"
                      [(ngModel)]="autoBidAmount"
                      [placeholder]="'Max amount above ' + auction.currentBid"
                    />
                    <button (click)="setAutoBid(auction.id)" [disabled]="autoBidLoading">
                      {{ autoBidLoading ? 'Saving...' : 'Set Auto-Bid' }}
                    </button>
                  </div>
                }
                @if (autoBidMessage) {
                  <div class="bid-success">{{ autoBidMessage }}</div>
                }
              </div>
            } @else if (!(isLoggedIn$ | async) && auction.status === 'Active') {
              <div class="login-prompt"><a routerLink="/login">Login to place a bid</a></div>
            }

            @if (buyNowSuccess) {
              <div class="bid-success">Congratulations! You bought this item. Payment confirmed.</div>
            }

            @if (auction.buyNowPrice && auction.status === 'Active') {
              <div class="buy-now">
                <button class="buy-now-btn" (click)="buyNow()">
                  Buy Now for {{ auction.buyNowPrice | currency }}
                </button>
              </div>
            }
          </div>

          @if (auction.bids && auction.bids.length) {
            <div class="bid-history">
              <h3>Bid History</h3>
              @for (bid of auction.bids; track bid.id) {
                <div class="bid-item" [class.own-bid]="bid.bidderId === (userId$ | async)">
                  <span>{{ bid.bidderId | slice:0:8 }}...</span>
                  <span>{{ bid.amount | currency }}</span>
                  <span>{{ bid.placedAt | date:'short' }}</span>
                </div>
              }
            </div>
          }
        </div>
      </div>
    }
  `
})
export class AuctionDetailComponent implements OnInit, OnDestroy {
  private store      = inject(Store);
  private route      = inject(ActivatedRoute);
  private hubService = inject(AuctionHubService);
  private http       = inject(HttpClient);
  private destroyRef = inject(DestroyRef);

  private readonly apiBase = environment.apiUrl;

  auction$ = this.store.select(selectSelectedAuction);
  isLoggedIn$ = this.store.select(selectIsLoggedIn);
  userId$ = this.store.select(selectUserId);

  bidAmount = 0;
  placing = false;
  bidError: string | null = null;
  bidSuccess = false;
  extended = false;
  justBid = false;
  isHighBidder = false;

  autoBidAmount = 0;
  autoBidMax: number | null = null;
  autoBidLoading = false;
  autoBidMessage = '';
  reserveNotMet = false;
  buyNowSuccess = false;

  private auctionId = '';

  ngOnInit() {
    this.auctionId = this.route.snapshot.paramMap.get('id')!;
    this.store.dispatch(AuctionsActions.loadAuction({ id: this.auctionId }));

    if (this.route.snapshot.queryParamMap.get('status') === 'buynow-success') {
      this.buyNowSuccess = true;
    }

    // Hub is already connected globally by auth effects — just join this auction's group
    this.hubService.joinAuction(this.auctionId)
      .catch(err => console.warn('SignalR joinAuction:', err));

    this.hubService.bidUpdate$.pipe(
      takeUntilDestroyed(this.destroyRef),
      withLatestFrom(this.store.select(selectUserId))
    ).subscribe(([update, userId]) => {
      if (update && update.auctionId === this.auctionId) {
        this.justBid = true;
        this.extended = update.extended;
        this.bidSuccess = true;
        this.placing = false;
        this.isHighBidder = update.bidderId === userId;
        setTimeout(() => { this.bidSuccess = false; this.extended = false; this.justBid = false; }, 3000);
        this.loadAutoBid();
      }
    });

    this.hubService.bidError$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(error => {
      if (error) {
        this.bidError = error;
        this.placing = false;
        setTimeout(() => (this.bidError = null), 3000);
      }
    });

    this.hubService.auctionEnded$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(data => {
      if (data && data.auctionId === this.auctionId) {
        this.reserveNotMet = data.reserveNotMet;
      }
    });

    this.loadAutoBid();
  }

  placeBid(auctionId: string, currentBid: number) {
    if (this.bidAmount <= currentBid) {
      this.bidError = `Bid must be higher than ${currentBid}`;
      return;
    }
    this.placing = true;
    this.bidError = null;
    this.hubService.placeBid(auctionId, this.bidAmount);
  }

  loadAutoBid() {
    this.store.select(selectIsLoggedIn).pipe(take(1)).subscribe(loggedIn => {
      if (!loggedIn) return;
      this.http.get<{ maxAmount: number } | null>(`${this.apiBase}/auto-bids/${this.auctionId}`)
        .subscribe({ next: res => (this.autoBidMax = res?.maxAmount ?? null), error: () => {} });
    });
  }

  setAutoBid(auctionId: string) {
    this.autoBidLoading = true;
    this.autoBidMessage = '';
    this.http.post(`${this.apiBase}/auto-bids`, { auctionId, maxAmount: this.autoBidAmount })
      .subscribe({
        next: () => {
          this.autoBidMax = this.autoBidAmount;
          this.autoBidLoading = false;
          this.autoBidMessage = 'Auto-bid set!';
          setTimeout(() => (this.autoBidMessage = ''), 3000);
        },
        error: (err) => {
          this.autoBidMessage = err?.error?.error ?? 'Failed to set auto-bid';
          this.autoBidLoading = false;
          setTimeout(() => (this.autoBidMessage = ''), 3000);
        }
      });
  }

  cancelAutoBid(auctionId: string) {
    this.autoBidLoading = true;
    this.http.delete(`${this.apiBase}/auto-bids/${auctionId}`)
      .subscribe({
        next: () => { this.autoBidMax = null; this.autoBidLoading = false; },
        error: () => { this.autoBidLoading = false; }
      });
  }

  buyNow() {
    this.http.post<{ transactionId: string; paymentUrl: string }>(
      `${this.apiBase}/payments/buy-now`, { auctionId: this.auctionId }
    ).subscribe({
      next: res => { window.location.href = res.paymentUrl; },
      error: err => {
        this.bidError = err?.error?.error ?? 'Buy Now failed. Please try again.';
        setTimeout(() => (this.bidError = null), 4000);
      }
    });
  }

  ngOnDestroy() {
    this.hubService.leaveAuction(this.auctionId);
    this.store.dispatch(AuctionsActions.clearSelected());
  }
}
