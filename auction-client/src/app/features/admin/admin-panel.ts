import { Component, OnInit, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe, CurrencyPipe, DecimalPipe } from '@angular/common';
import { environment } from '../../../environments/environment';

interface AdminStats {
  totalUsers: number;
  totalAuctions: number;
  active: number;
  ended: number;
  draft: number;
  totalBids: number;
  totalRevenue: number;
  buyNowRevenue: number;
  combinedRevenue: number;
}

interface AdminUser {
  id: string;
  username: string;
  email: string;
  role: string;
  createdAt: string;
  phoneVerified: boolean;
  bidBalance: number;
}

interface AdminAuction {
  id: string;
  title: string;
  itemType: string;
  status: string;
  currentBid: number;
  buyNowPrice: number | null;
  bidCount: number;
  startsAt: string;
  endsAt: string;
  sellerId: string;
  createdAt: string;
}

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [DatePipe, CurrencyPipe, DecimalPipe],
  template: `
    <div class="admin-container">
      <div class="admin-header">
        <h1>Admin Panel</h1>
        <p class="admin-subtitle">Platform management &amp; oversight</p>
      </div>

      <!-- Stats Cards -->
      @if (stats) {
        <div class="stats-grid">
          <div class="stat-card stat-users">
            <div class="stat-icon">👥</div>
            <div class="stat-body">
              <div class="stat-value">{{ stats.totalUsers }}</div>
              <div class="stat-label">Total Users</div>
            </div>
          </div>
          <div class="stat-card stat-active">
            <div class="stat-icon">🔥</div>
            <div class="stat-body">
              <div class="stat-value">{{ stats.active }}</div>
              <div class="stat-label">Active Auctions</div>
            </div>
          </div>
          <div class="stat-card stat-bids">
            <div class="stat-icon">🔨</div>
            <div class="stat-body">
              <div class="stat-value">{{ stats.totalBids | number }}</div>
              <div class="stat-label">Total Bids Placed</div>
            </div>
          </div>
          <div class="stat-card stat-revenue">
            <div class="stat-icon">💰</div>
            <div class="stat-body">
              <div class="stat-value">{{ stats.combinedRevenue | currency:'GEL':'symbol':'1.2-2' }}</div>
              <div class="stat-label">Total Revenue</div>
            </div>
          </div>
          <div class="stat-card stat-draft">
            <div class="stat-icon">📋</div>
            <div class="stat-body">
              <div class="stat-value">{{ stats.draft }}</div>
              <div class="stat-label">Draft Auctions</div>
            </div>
          </div>
          <div class="stat-card stat-ended">
            <div class="stat-icon">✅</div>
            <div class="stat-body">
              <div class="stat-value">{{ stats.ended }}</div>
              <div class="stat-label">Completed</div>
            </div>
          </div>
        </div>
      }

      <!-- Tab Navigation -->
      <div class="tab-bar">
        <button class="tab-btn" [class.active]="activeTab === 'auctions'" (click)="activeTab = 'auctions'">
          Auctions ({{ auctions.length }})
        </button>
        <button class="tab-btn" [class.active]="activeTab === 'users'" (click)="activeTab = 'users'">
          Users ({{ users.length }})
        </button>
      </div>

      @if (actionMessage) {
        <div class="action-toast" [class.error]="actionIsError">{{ actionMessage }}</div>
      }

      <!-- Auctions Tab -->
      @if (activeTab === 'auctions') {
        <div class="table-section">
          <div class="table-toolbar">
            <div class="filter-group">
              @for (s of ['All','Draft','Active','Ended','Cancelled']; track s) {
                <button class="filter-btn" [class.active]="auctionFilter === s" (click)="auctionFilter = s">
                  {{ s }}
                </button>
              }
            </div>
          </div>

          <div class="table-wrapper">
            <table class="admin-table">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Current Bid</th>
                  <th>Bids</th>
                  <th>Ends At</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (a of filteredAuctions; track a.id) {
                  <tr>
                    <td class="cell-title">{{ a.title }}</td>
                    <td><span class="tag">{{ a.itemType }}</span></td>
                    <td><span class="status-badge" [class]="'status-' + a.status.toLowerCase()">{{ a.status }}</span></td>
                    <td>{{ a.currentBid | currency:'GEL':'symbol':'1.2-2' }}</td>
                    <td class="cell-center">{{ a.bidCount }}</td>
                    <td class="cell-date">{{ a.endsAt | date:'MMM d, y HH:mm' }}</td>
                    <td class="cell-actions">
                      @if (a.status === 'Draft') {
                        <button class="action-btn activate-btn" (click)="activateAuction(a.id)" title="Force activate">
                          ▶ Activate
                        </button>
                      }
                      @if (a.status === 'Draft' || a.status === 'Active') {
                        <button class="action-btn cancel-btn" (click)="cancelAuction(a.id)" title="Cancel auction">
                          ✕ Cancel
                        </button>
                      }
                      @if (a.status === 'Ended' || a.status === 'Cancelled') {
                        <span class="cell-muted">—</span>
                      }
                    </td>
                  </tr>
                }
                @if (filteredAuctions.length === 0) {
                  <tr><td colspan="7" class="cell-empty">No auctions found.</td></tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }

      <!-- Users Tab -->
      @if (activeTab === 'users') {
        <div class="table-section">
          <div class="table-wrapper">
            <table class="admin-table">
              <thead>
                <tr>
                  <th>Username</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Bid Credits</th>
                  <th>Phone</th>
                  <th>Joined</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (u of users; track u.id) {
                  <tr [class.row-admin]="u.role === 'Admin'">
                    <td class="cell-username">{{ u.username }}</td>
                    <td class="cell-email">{{ u.email }}</td>
                    <td>
                      <span class="role-badge" [class.role-admin]="u.role === 'Admin'">
                        {{ u.role }}
                      </span>
                    </td>
                    <td class="cell-center">{{ u.bidBalance }}</td>
                    <td class="cell-center">{{ u.phoneVerified ? '✅' : '—' }}</td>
                    <td class="cell-date">{{ u.createdAt | date:'MMM d, y' }}</td>
                    <td class="cell-actions">
                      @if (u.role === 'Admin') {
                        <button class="action-btn demote-btn" (click)="setRole(u, 'User')">
                          Remove Admin
                        </button>
                      } @else {
                        <button class="action-btn promote-btn" (click)="setRole(u, 'Admin')">
                          Make Admin
                        </button>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .admin-container {
      max-width: 1400px;
      margin: 0 auto;
      padding: 2rem 1.5rem;
    }

    .admin-header { margin-bottom: 2rem; }
    .admin-header h1 { font-size: 2rem; font-weight: 700; margin: 0 0 .25rem; }
    .admin-subtitle { color: var(--text-muted, #888); margin: 0; }

    /* Stats grid */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 1rem;
      margin-bottom: 2.5rem;
    }

    .stat-card {
      background: var(--card-bg, #1a1a2e);
      border: 1px solid var(--border, #2a2a4a);
      border-radius: 12px;
      padding: 1.25rem;
      display: flex;
      align-items: center;
      gap: 1rem;
      transition: transform .15s;
    }
    .stat-card:hover { transform: translateY(-2px); }

    .stat-icon { font-size: 2rem; line-height: 1; }
    .stat-value { font-size: 1.6rem; font-weight: 700; line-height: 1.1; }
    .stat-label { font-size: .8rem; color: var(--text-muted, #888); margin-top: .2rem; }

    .stat-users   { border-color: #6366f1; }
    .stat-active  { border-color: #f59e0b; }
    .stat-bids    { border-color: #8b5cf6; }
    .stat-revenue { border-color: #10b981; }
    .stat-draft   { border-color: #64748b; }
    .stat-ended   { border-color: #06b6d4; }

    /* Tabs */
    .tab-bar {
      display: flex;
      gap: .5rem;
      margin-bottom: 1.5rem;
      border-bottom: 2px solid var(--border, #2a2a4a);
      padding-bottom: .5rem;
    }
    .tab-btn {
      background: none;
      border: none;
      color: var(--text-muted, #888);
      font-size: 1rem;
      font-weight: 500;
      padding: .5rem 1.25rem;
      cursor: pointer;
      border-radius: 6px 6px 0 0;
      transition: color .15s, background .15s;
    }
    .tab-btn.active { color: var(--accent, #7c6af7); background: var(--card-bg, #1a1a2e); }
    .tab-btn:hover:not(.active) { color: var(--text, #eee); }

    /* Toast */
    .action-toast {
      padding: .75rem 1.25rem;
      border-radius: 8px;
      margin-bottom: 1rem;
      background: #10b98120;
      border: 1px solid #10b981;
      color: #10b981;
      font-weight: 500;
    }
    .action-toast.error { background: #ef444420; border-color: #ef4444; color: #ef4444; }

    /* Table */
    .table-toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }
    .filter-group { display: flex; gap: .4rem; flex-wrap: wrap; }
    .filter-btn {
      background: var(--card-bg, #1a1a2e);
      border: 1px solid var(--border, #2a2a4a);
      color: var(--text-muted, #888);
      padding: .3rem .9rem;
      border-radius: 20px;
      font-size: .85rem;
      cursor: pointer;
      transition: all .15s;
    }
    .filter-btn.active, .filter-btn:hover {
      border-color: var(--accent, #7c6af7);
      color: var(--accent, #7c6af7);
    }

    .table-wrapper { overflow-x: auto; }

    .admin-table {
      width: 100%;
      border-collapse: collapse;
      font-size: .9rem;
    }
    .admin-table th {
      text-align: left;
      padding: .75rem 1rem;
      color: var(--text-muted, #888);
      font-weight: 600;
      font-size: .8rem;
      text-transform: uppercase;
      letter-spacing: .05em;
      border-bottom: 2px solid var(--border, #2a2a4a);
      white-space: nowrap;
    }
    .admin-table td {
      padding: .8rem 1rem;
      border-bottom: 1px solid var(--border, #1e1e3a);
      vertical-align: middle;
    }
    .admin-table tr:hover td { background: var(--card-bg, #1a1a2e); }
    .admin-table tr.row-admin td { background: #7c6af708; }

    .cell-title { font-weight: 500; max-width: 220px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .cell-center { text-align: center; }
    .cell-date { color: var(--text-muted, #888); font-size: .85rem; white-space: nowrap; }
    .cell-muted { color: var(--text-muted, #888); }
    .cell-empty { text-align: center; color: var(--text-muted, #888); padding: 2rem; }
    .cell-username { font-weight: 600; }
    .cell-email { color: var(--text-muted, #888); font-size: .85rem; }
    .cell-actions { display: flex; gap: .4rem; align-items: center; }

    .tag {
      background: var(--surface, #252540);
      padding: .2rem .6rem;
      border-radius: 4px;
      font-size: .78rem;
      color: var(--text-muted, #aaa);
    }

    /* Status badges */
    .status-badge {
      padding: .25rem .65rem;
      border-radius: 20px;
      font-size: .78rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: .04em;
    }
    .status-active    { background: #f59e0b20; color: #f59e0b; border: 1px solid #f59e0b40; }
    .status-draft     { background: #64748b20; color: #94a3b8; border: 1px solid #64748b40; }
    .status-ended     { background: #06b6d420; color: #06b6d4; border: 1px solid #06b6d440; }
    .status-cancelled { background: #ef444420; color: #ef4444; border: 1px solid #ef444440; }

    /* Role badges */
    .role-badge {
      padding: .25rem .7rem;
      border-radius: 20px;
      font-size: .78rem;
      font-weight: 600;
      background: #64748b20;
      color: #94a3b8;
      border: 1px solid #64748b40;
    }
    .role-badge.role-admin { background: #7c6af720; color: #a78bfa; border-color: #7c6af740; }

    /* Action buttons */
    .action-btn {
      padding: .3rem .75rem;
      border-radius: 6px;
      font-size: .8rem;
      font-weight: 500;
      cursor: pointer;
      border: 1px solid transparent;
      transition: all .15s;
      white-space: nowrap;
    }
    .activate-btn { background: #10b98120; color: #10b981; border-color: #10b98140; }
    .activate-btn:hover { background: #10b98130; }
    .cancel-btn { background: #ef444420; color: #ef4444; border-color: #ef444440; }
    .cancel-btn:hover { background: #ef444430; }
    .promote-btn { background: #7c6af720; color: #a78bfa; border-color: #7c6af740; }
    .promote-btn:hover { background: #7c6af730; }
    .demote-btn { background: #64748b20; color: #94a3b8; border-color: #64748b40; }
    .demote-btn:hover { background: #64748b30; }
  `]
})
export class AdminPanelComponent implements OnInit {
  private http = inject(HttpClient);
  private api  = environment.apiUrl;

  stats: AdminStats | null = null;
  users: AdminUser[]       = [];
  auctions: AdminAuction[] = [];

  activeTab     = 'auctions';
  auctionFilter = 'All';
  actionMessage = '';
  actionIsError = false;

  get filteredAuctions(): AdminAuction[] {
    if (this.auctionFilter === 'All') return this.auctions;
    return this.auctions.filter(a => a.status === this.auctionFilter);
  }

  ngOnInit() {
    this.loadStats();
    this.loadAuctions();
    this.loadUsers();
  }

  loadStats() {
    this.http.get<AdminStats>(`${this.api}/admin/stats`)
      .subscribe({ next: s => (this.stats = s), error: () => {} });
  }

  loadAuctions() {
    this.http.get<AdminAuction[]>(`${this.api}/admin/auctions`)
      .subscribe({ next: a => (this.auctions = a), error: () => {} });
  }

  loadUsers() {
    this.http.get<AdminUser[]>(`${this.api}/admin/users`)
      .subscribe({ next: u => (this.users = u), error: () => {} });
  }

  activateAuction(id: string) {
    this.http.post(`${this.api}/admin/auctions/${id}/activate`, {})
      .subscribe({
        next: () => {
          const a = this.auctions.find(x => x.id === id);
          if (a) a.status = 'Active';
          this.loadStats();
          this.showMessage('Auction activated.');
        },
        error: err => this.showMessage(err?.error?.error ?? 'Failed to activate.', true)
      });
  }

  cancelAuction(id: string) {
    this.http.delete(`${this.api}/admin/auctions/${id}`)
      .subscribe({
        next: () => {
          const a = this.auctions.find(x => x.id === id);
          if (a) a.status = 'Cancelled';
          this.loadStats();
          this.showMessage('Auction cancelled.');
        },
        error: err => this.showMessage(err?.error?.error ?? 'Failed to cancel.', true)
      });
  }

  setRole(user: AdminUser, role: string) {
    this.http.patch(`${this.api}/admin/users/${user.id}/role`, { role })
      .subscribe({
        next: () => {
          user.role = role;
          this.loadStats();
          this.showMessage(`${user.username} is now ${role}.`);
        },
        error: err => this.showMessage(err?.error?.error ?? 'Failed to update role.', true)
      });
  }

  private showMessage(msg: string, isError = false) {
    this.actionMessage = msg;
    this.actionIsError = isError;
    setTimeout(() => (this.actionMessage = ''), 3500);
  }
}
