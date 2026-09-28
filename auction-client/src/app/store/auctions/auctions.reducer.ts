import { createReducer, on } from '@ngrx/store';
import { Auction } from '../../core/services/auction';
import * as AuctionsActions from './auctions.actions';

export interface AuctionsState {
  list: Auction[];
  selected: Auction | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
}

const initialState: AuctionsState = {
  list: [],
  selected: null,
  loading: false,
  error: null,
  totalCount: 0,
  currentPage: 1,
  pageSize: 12
};

export const auctionsReducer = createReducer(
  initialState,
  on(AuctionsActions.loadAuctions, AuctionsActions.loadAuction, state => ({ ...state, loading: true, error: null })),
  on(AuctionsActions.loadAuctionsSuccess, (state, { auctions, totalCount, page, pageSize }) => ({
    ...state,
    list: auctions,
    totalCount,
    currentPage: page,
    pageSize,
    loading: false
  })),
  on(AuctionsActions.loadAuctionsFailure, AuctionsActions.loadAuctionFailure, (state, { error }) => ({ ...state, error, loading: false })),
  on(AuctionsActions.loadAuctionSuccess, (state, { auction }) => ({ ...state, selected: auction, loading: false })),
  on(AuctionsActions.createAuctionSuccess, (state, { auction }) => ({
    ...state,
    list: [auction, ...state.list],
    totalCount: state.totalCount + 1
  })),
  on(AuctionsActions.clearSelected, state => ({ ...state, selected: null })),
  on(AuctionsActions.bidPlaced, (state, { update }) => {
    const patch = (a: Auction): Auction => a.id === update.auctionId
      ? { ...a, currentBid: update.newBid, bidCount: update.bidCount, endsAt: update.endsAt }
      : a;
    return {
      ...state,
      list: state.list.map(patch),
      selected: state.selected ? patch(state.selected) : null
    };
  }),
  on(AuctionsActions.auctionEnded, (state, { data }) => {
    const patch = (a: Auction): Auction => a.id === data.auctionId
      ? {
          ...a,
          status: 'Ended',
          currentBid: data.finalBid,
          highestBidderId: data.reserveNotMet ? null : data.highestBidderId
        }
      : a;
    return {
      ...state,
      list: state.list.map(patch),
      selected: state.selected ? patch(state.selected) : null
    };
  })
);
