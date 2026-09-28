import { describe, it, expect } from 'vitest';
import { auctionsReducer, AuctionsState } from './auctions.reducer';
import * as AuctionsActions from './auctions.actions';
import { BidUpdate, AuctionEnded } from '../../core/hubs/auction-hub';
import { Auction } from '../../core/services/auction';

const mockAuction: Auction = {
  id: 'auction-1',
  title: 'Test Auction',
  description: 'desc',
  itemType: 'Physical',
  startingPrice: 10,
  currentBid: 10,
  buyNowPrice: 0,
  reservePrice: 0,
  status: 'Active',
  startsAt: '2025-01-01T00:00:00Z',
  endsAt: '2025-12-31T00:00:00Z',
  sellerId: 'seller-1',
  highestBidderId: null,
  bidCount: 2,
  bids: [],
  imageUrl: null,
};

const initial: AuctionsState = {
  list: [mockAuction],
  selected: null,
  loading: false,
  error: null,
  totalCount: 1,
  currentPage: 1,
  pageSize: 12,
};

describe('auctionsReducer', () => {
  it('sets loading=true on loadAuctions', () => {
    const state = auctionsReducer(initial, AuctionsActions.loadAuctions({ page: 1 }));
    expect(state.loading).toBe(true);
    expect(state.error).toBeNull();
  });

  it('updates list + pagination on loadAuctionsSuccess', () => {
    const state = auctionsReducer(
      initial,
      AuctionsActions.loadAuctionsSuccess({
        auctions: [mockAuction],
        totalCount: 50,
        page: 2,
        pageSize: 12,
      })
    );
    expect(state.list).toHaveLength(1);
    expect(state.totalCount).toBe(50);
    expect(state.currentPage).toBe(2);
    expect(state.loading).toBe(false);
  });

  it('sets error on loadAuctionsFailure', () => {
    const state = auctionsReducer(
      initial,
      AuctionsActions.loadAuctionsFailure({ error: 'oops' })
    );
    expect(state.error).toBe('oops');
    expect(state.loading).toBe(false);
  });

  it('updates currentBid, bidCount and endsAt on bidPlaced', () => {
    const update: BidUpdate = {
      auctionId: 'auction-1',
      newBid: 25,
      bidderId: 'user-2',
      bidCount: 5,
      endsAt: '2025-12-31T01:00:00Z',
      extended: false,
    };
    const state = auctionsReducer(initial, AuctionsActions.bidPlaced({ update }));
    expect(state.list[0].currentBid).toBe(25);
    expect(state.list[0].bidCount).toBe(5);
    expect(state.list[0].endsAt).toBe('2025-12-31T01:00:00Z');
  });

  it('ignores bidPlaced for a different auction', () => {
    const update: BidUpdate = {
      auctionId: 'other-auction',
      newBid: 99,
      bidderId: 'user-2',
      bidCount: 10,
      endsAt: '2025-12-31T00:00:00Z',
      extended: false,
    };
    const state = auctionsReducer(initial, AuctionsActions.bidPlaced({ update }));
    expect(state.list[0].currentBid).toBe(10); // unchanged
  });

  it('marks auction Ended with winner on auctionEnded (reserve met)', () => {
    const data: AuctionEnded = {
      auctionId: 'auction-1',
      finalBid: 30,
      highestBidderId: 'user-2',
      reserveNotMet: false,
    };
    const state = auctionsReducer(initial, AuctionsActions.auctionEnded({ data }));
    expect(state.list[0].status).toBe('Ended');
    expect(state.list[0].currentBid).toBe(30);
    expect(state.list[0].highestBidderId).toBe('user-2');
  });

  it('clears highestBidderId on auctionEnded when reserve not met', () => {
    const stateWithWinner = {
      ...initial,
      list: [{ ...mockAuction, highestBidderId: 'user-2' }],
    };
    const data: AuctionEnded = {
      auctionId: 'auction-1',
      finalBid: 5,
      highestBidderId: null,
      reserveNotMet: true,
    };
    const state = auctionsReducer(stateWithWinner, AuctionsActions.auctionEnded({ data }));
    expect(state.list[0].status).toBe('Ended');
    expect(state.list[0].highestBidderId).toBeNull();
  });

  it('also patches selected auction on bidPlaced', () => {
    const stateWithSelected = { ...initial, selected: { ...mockAuction } };
    const update: BidUpdate = {
      auctionId: 'auction-1',
      newBid: 42,
      bidderId: 'user-2',
      bidCount: 7,
      endsAt: '2025-12-31T00:00:00Z',
      extended: false,
    };
    const state = auctionsReducer(stateWithSelected, AuctionsActions.bidPlaced({ update }));
    expect(state.selected?.currentBid).toBe(42);
    expect(state.selected?.bidCount).toBe(7);
  });

  it('clears selected on clearSelected', () => {
    const stateWithSelected = { ...initial, selected: mockAuction };
    const state = auctionsReducer(stateWithSelected, AuctionsActions.clearSelected());
    expect(state.selected).toBeNull();
  });
});
