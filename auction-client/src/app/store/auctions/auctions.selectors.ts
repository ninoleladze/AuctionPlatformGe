import { createSelector, createFeatureSelector } from '@ngrx/store';
import { AuctionsState } from './auctions.reducer';
import { selectUserId } from '../auth/auth.selectors';

export const selectAuctionsState = createFeatureSelector<AuctionsState>('auctions');
export const selectAllAuctions = createSelector(selectAuctionsState, s => s.list);
export const selectSelectedAuction = createSelector(selectAuctionsState, s => s.selected);
export const selectAuctionsLoading = createSelector(selectAuctionsState, s => s.loading);
export const selectAuctionsError = createSelector(selectAuctionsState, s => s.error);
export const selectTotalCount = createSelector(selectAuctionsState, s => s.totalCount);
export const selectCurrentPage = createSelector(selectAuctionsState, s => s.currentPage);
export const selectPageSize = createSelector(selectAuctionsState, s => s.pageSize);
export const selectTotalPages = createSelector(
  selectTotalCount,
  selectPageSize,
  (total, size) => Math.ceil(total / size)
);

export const selectMyAuctions = createSelector(
  selectAllAuctions,
  selectUserId,
  (auctions, userId) => auctions.filter(a => a.sellerId === userId)
);

export const selectActiveAuctions = createSelector(
  selectAllAuctions,
  auctions => auctions.filter(a => a.status === 'Active')
);
