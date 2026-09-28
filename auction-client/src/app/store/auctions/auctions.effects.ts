import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, filter, map, of, switchMap, tap, withLatestFrom } from 'rxjs';
import { AuctionService } from '../../core/services/auction';
import { AuctionHubService } from '../../core/hubs/auction-hub';
import * as AuctionsActions from './auctions.actions';
import * as AuthActions from '../auth/auth.actions';
import { selectCurrentPage, selectPageSize } from './auctions.selectors';

@Injectable()
export class AuctionsEffects {
  private actions$      = inject(Actions);
  private auctionService = inject(AuctionService);
  private hubService    = inject(AuctionHubService);
  private router        = inject(Router);
  private store         = inject(Store);

  loadAuctions$ = createEffect(() => this.actions$.pipe(
    ofType(AuctionsActions.loadAuctions),
    switchMap(({ page, pageSize }) =>
      this.auctionService.getAll(page ?? 1, pageSize ?? 12).pipe(
        map(result => AuctionsActions.loadAuctionsSuccess({
          auctions: result.items,
          totalCount: result.totalCount,
          page: result.page,
          pageSize: result.pageSize
        })),
        catchError(() => of(AuctionsActions.loadAuctionsFailure({ error: 'Failed to load auctions' })))
      )
    )
  ));

  loadAuction$ = createEffect(() => this.actions$.pipe(
    ofType(AuctionsActions.loadAuction),
    switchMap(({ id }) =>
      this.auctionService.getById(id).pipe(
        map(auction => AuctionsActions.loadAuctionSuccess({ auction })),
        catchError(() => of(AuctionsActions.loadAuctionFailure({ error: 'Auction not found' })))
      )
    )
  ));

  createAuction$ = createEffect(() => this.actions$.pipe(
    ofType(AuctionsActions.createAuction),
    switchMap(({ dto }) =>
      this.auctionService.create(dto).pipe(
        map(auction => AuctionsActions.createAuctionSuccess({ auction })),
        catchError(err => of(AuctionsActions.createAuctionFailure({
          error: (err as { error?: { error?: string } })?.error?.error ?? 'Failed to create auction'
        })))
      )
    )
  ));

  navigateAfterCreate$ = createEffect(() => this.actions$.pipe(
    ofType(AuctionsActions.createAuctionSuccess),
    tap(({ auction }) => this.router.navigate(['/auctions', auction.id]))
  ), { dispatch: false });

  // Bridge SignalR events -> store
  bidPlaced$ = createEffect(() => this.hubService.bidUpdate$.pipe(
    filter((update): update is NonNullable<typeof update> => !!update),
    map(update => AuctionsActions.bidPlaced({ update }))
  ));

  auctionEnded$ = createEffect(() => this.hubService.auctionEnded$.pipe(
    filter((data): data is NonNullable<typeof data> => !!data),
    map(data => AuctionsActions.auctionEnded({ data }))
  ));

  // Reload current page when a Draft auction becomes Active
  auctionActivated$ = createEffect(() => this.hubService.auctionActivated$.pipe(
    filter((id): id is string => !!id),
    withLatestFrom(
      this.store.select(selectCurrentPage),
      this.store.select(selectPageSize)
    ),
    map(([, page, pageSize]) => AuctionsActions.loadAuctions({ page, pageSize }))
  ));

  // Decrement bid balance only when the current user placed the bid
  updateBalance$ = createEffect(() => this.actions$.pipe(
    ofType(AuctionsActions.bidPlaced),
    map(({ update }) => {
      const stored = localStorage.getItem('user');
      if (!stored) return AuthActions.updateBidBalance({ bidBalance: 0 });
      const user = JSON.parse(stored) as { userId?: string; bidBalance?: number };
      if (update.bidderId !== user.userId) {
        return AuthActions.updateBidBalance({ bidBalance: user.bidBalance ?? 0 });
      }
      const newBalance = Math.max(0, (user.bidBalance ?? 0) - 1);
      user.bidBalance = newBalance;
      localStorage.setItem('user', JSON.stringify(user));
      return AuthActions.updateBidBalance({ bidBalance: newBalance });
    })
  ));
}
