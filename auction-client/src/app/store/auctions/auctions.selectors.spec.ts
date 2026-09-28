import { describe, it, expect } from 'vitest';
import {
  selectAllAuctions,
  selectTotalPages,
  selectMyAuctions,
  selectActiveAuctions,
} from './auctions.selectors';
import { AuctionsState } from './auctions.reducer';
import { Auction } from '../../core/services/auction';

const makeAuction = (overrides: Partial<Auction> = {}): Auction => ({
  id: 'auction-1',
  title: 'Test',
  description: '',
  itemType: 'Physical',
  startingPrice: 10,
  currentBid: 10,
  buyNowPrice: 0,
  reservePrice: 0,
  status: 'Active',
  startsAt: '',
  endsAt: '',
  sellerId: 'seller-1',
  highestBidderId: null,
  bidCount: 0,
  bids: [],
  imageUrl: null,
  ...overrides,
});

const makeState = (
  auctionsOverrides: Partial<AuctionsState> = {},
  userId: string | null = null
) => ({
  auctions: {
    list: [],
    selected: null,
    loading: false,
    error: null,
    totalCount: 0,
    currentPage: 1,
    pageSize: 12,
    ...auctionsOverrides,
  },
  auth: {
    user: userId ? { userId, username: 'u', email: 'e', token: 't', bidBalance: 0 } : null,
    loading: false,
    error: null,
  },
});

describe('selectAllAuctions', () => {
  it('returns list from state', () => {
    const a = makeAuction();
    const state = makeState({ list: [a] });
    expect(selectAllAuctions(state as any)).toEqual([a]);
  });
});

describe('selectTotalPages', () => {
  it('rounds up correctly', () => {
    expect(selectTotalPages(makeState({ totalCount: 25, pageSize: 12 }) as any)).toBe(3);
  });

  it('returns 1 for exactly one page', () => {
    expect(selectTotalPages(makeState({ totalCount: 12, pageSize: 12 }) as any)).toBe(1);
  });

  it('returns 0 when empty', () => {
    expect(selectTotalPages(makeState({ totalCount: 0, pageSize: 12 }) as any)).toBe(0);
  });
});

describe('selectMyAuctions', () => {
  it('returns only auctions belonging to current user', () => {
    const mine  = makeAuction({ id: '1', sellerId: 'user-1' });
    const other = makeAuction({ id: '2', sellerId: 'user-2' });
    const state = makeState({ list: [mine, other] }, 'user-1');
    expect(selectMyAuctions(state as any)).toEqual([mine]);
  });

  it('returns empty array when user has no auctions', () => {
    const other = makeAuction({ id: '1', sellerId: 'user-2' });
    const state = makeState({ list: [other] }, 'user-1');
    expect(selectMyAuctions(state as any)).toEqual([]);
  });

  it('returns empty array when not logged in', () => {
    const a = makeAuction({ sellerId: 'user-1' });
    const state = makeState({ list: [a] }, null);
    expect(selectMyAuctions(state as any)).toEqual([]);
  });
});

describe('selectActiveAuctions', () => {
  it('filters to only Active status', () => {
    const active = makeAuction({ id: '1', status: 'Active' });
    const ended  = makeAuction({ id: '2', status: 'Ended' });
    const draft  = makeAuction({ id: '3', status: 'Draft' });
    const state  = makeState({ list: [active, ended, draft] });
    expect(selectActiveAuctions(state as any)).toEqual([active]);
  });
});
