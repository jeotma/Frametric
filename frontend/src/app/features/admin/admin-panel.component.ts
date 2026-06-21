import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../core/api/api/admin.service';
import { DatabaseStatsDto } from '../../core/api/model/database-stats-dto';
import { ProviderDiagnosticsDto } from '../../core/api/model/provider-diagnostics-dto';
import { LogEntryDto } from '../../core/api/model/log-entry-dto';
import { UserDto } from '../../core/api/model/user-dto';
import { PurgeOrphanResultDto } from '../../core/api/model/purge-orphan-result-dto';

type AdminTab = 'database' | 'providers' | 'users' | 'logs';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="admin-container">
      <div class="header-section">
        <h1 class="page-title">
          <svg class="icon title-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19.14 12.94c.04-.3.06-.61.06-.94 0-.32-.02-.64-.07-.94l2.03-1.58c.18-.14.23-.41.12-.61l-1.92-3.32c-.12-.22-.37-.29-.59-.22l-2.39.96c-.5-.38-1.03-.7-1.62-.94l-.36-2.54c-.04-.24-.24-.41-.48-.41h-3.84c-.24 0-.43.17-.47.41l-.36 2.54c-.59.24-1.13.57-1.62.94l-2.39-.96c-.22-.08-.47 0-.59.22L2.74 8.87c-.12.21-.08.47.12.61l2.03 1.58c-.05.3-.09.63-.09.94s.02.64.07.94l-2.03 1.58c-.18.14-.23.41-.12.61l1.92 3.32c.12.22.37.29.59.22l2.39-.96c.5.38 1.03.7 1.62.94l.36 2.54c.05.24.24.41.48.41h3.84c.24 0 .44-.17.47-.41l.36-2.54c.59-.24 1.13-.56 1.62-.94l2.39.96c.22.08.47 0 .59-.22l1.92-3.32c.12-.22.07-.47-.12-.61l-2.01-1.58zM12 15.6c-1.98 0-3.6-1.62-3.6-3.6s1.62-3.6 3.6-3.6 3.6 1.62 3.6 3.6-1.62 3.6-3.6 3.6z"/>
          </svg>
          Configuration Panel
        </h1>
        <p class="subtitle">System diagnostics, background queue, database status, and user management.</p>
      </div>

      <!-- Navigation Tabs -->
      <div class="tabs-nav">
        <button [class.active]="activeTab() === 'database'" (click)="setTab('database')">
          Database & Queue
        </button>
        <button [class.active]="activeTab() === 'providers'" (click)="setTab('providers')">
          API Health
        </button>
        <button [class.active]="activeTab() === 'users'" (click)="setTab('users')">
          User Management
        </button>
        <button [class.active]="activeTab() === 'logs'" (click)="setTab('logs')">
          Diagnostic Logs
        </button>
      </div>

      <!-- Error / Success Banners -->
      <div *ngIf="errorMessage()" class="alert-banner error crosshair-bracket">
        <span class="banner-title">SYSTEM FAULT:</span>
        <span class="banner-text">{{ errorMessage() }}</span>
        <button class="dismiss-btn" (click)="errorMessage.set(null)">&times;</button>
      </div>

      <div *ngIf="successMessage()" class="alert-banner success crosshair-bracket">
        <span class="banner-title">SUCCESS:</span>
        <span class="banner-text">{{ successMessage() }}</span>
        <button class="dismiss-btn" (click)="successMessage.set(null)">&times;</button>
      </div>

      <!-- Loading State -->
      <div *ngIf="isLoading()" class="loading-overlay">
        <div class="spinner"></div>
        <p>Fetching diagnostic data...</p>
      </div>

      <!-- TAB CONTENT -->
      <div class="tab-content" [ngSwitch]="activeTab()">
        
        <!-- DATABASE & QUEUE TAB -->
        <div *ngSwitchCase="'database'" class="tab-pane animate-fade-in">
          <div class="pane-grid">
            
            <div class="card crosshair-bracket">
              <h2 class="card-title">Enrichment Queue Status</h2>
              <div class="stats-list" *ngIf="stats()">
                <div class="stat-row">
                  <span class="stat-label">Pending Ingestions</span>
                  <span class="stat-val" [class.highlight]="stats()!.pendingMovies > 0">{{ stats()!.pendingMovies }}</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">Completed Ingestions</span>
                  <span class="stat-val completed">{{ stats()!.completedMovies }}</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">Transient Failures</span>
                  <span class="stat-val failed">{{ stats()!.failedMovies }}</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">Not Found on TMDB</span>
                  <span class="stat-val failed">{{ stats()!.notFoundMovies }}</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">Permanently Failed</span>
                  <span class="stat-val critical">{{ stats()!.permanentlyFailedMovies }}</span>
                </div>
              </div>

              <!-- Action Controls -->
              <div class="card-actions">
                <button class="btn btn-record" (click)="runRetry(false)" [disabled]="actionPending()">
                  Retry Failed Ingestions
                </button>
                <button class="btn btn-muted" (click)="runRetry(true)" [disabled]="actionPending()">
                  Reset & Retry Permanently Failed
                </button>
              </div>
            </div>

            <div class="card crosshair-bracket">
              <h2 class="card-title">Database Overview</h2>
              <div class="stats-list" *ngIf="stats()">
                <div class="stat-row">
                  <span class="stat-label">Total Movies</span>
                  <span class="stat-val">{{ stats()!.totalMovies }}</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">TV Shows / Miniseries</span>
                  <span class="stat-val">{{ stats()!.totalTvShows }}</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">Logged Watch Entries</span>
                  <span class="stat-val">{{ stats()!.totalDiaryEntries }}</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">Cached Genres</span>
                  <span class="stat-val">{{ stats()!.totalGenres }}</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">Cached Directors</span>
                  <span class="stat-val">{{ stats()!.totalDirectors }}</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">Cached Actors</span>
                  <span class="stat-val">{{ stats()!.totalActors }}</span>
                </div>
              </div>

              <!-- Maintenance Controls -->
              <div class="card-actions">
                <button class="btn btn-sepia" (click)="purgeOrphans()" [disabled]="actionPending()">
                  Clean Orphan Records
                </button>
                <button class="btn btn-muted" (click)="clearCache()" [disabled]="actionPending()">
                  Clear Recommendation Cache
                </button>
              </div>
            </div>

          </div>
        </div>

        <!-- API HEALTH TAB -->
        <div *ngSwitchCase="'providers'" class="tab-pane animate-fade-in">
          <div class="health-container" *ngIf="diagnostics()">
            
            <div class="health-card crosshair-bracket">
              <div class="provider-header">
                <span class="provider-name">Frametric Backend & Database</span>
                <span class="health-badge" [class.healthy]="diagnostics()!.backendStatus === 'Healthy'" [class.unhealthy]="diagnostics()!.backendStatus !== 'Healthy'">
                  {{ diagnostics()!.backendStatus }}
                </span>
              </div>
              <div class="provider-body">
                <div class="diag-row">
                  <span class="diag-label">DB Ping Latency</span>
                  <span class="diag-val">{{ diagnostics()!.backendLatencyMs }} ms</span>
                </div>
                <div class="diag-row">
                  <span class="diag-label">Database Status</span>
                  <span class="diag-val">{{ diagnostics()!.backendStatus === 'Healthy' ? 'OPERATIONAL' : 'FAULT' }}</span>
                </div>
              </div>
            </div>

            <div class="health-card crosshair-bracket">
              <div class="provider-header">
                <span class="provider-name">The Movie Database (TMDB)</span>
                <span class="health-badge" [class.healthy]="diagnostics()!.tmdbStatus === 'Healthy'" [class.unhealthy]="diagnostics()!.tmdbStatus !== 'Healthy'">
                  {{ diagnostics()!.tmdbStatus }}
                </span>
              </div>
              <div class="provider-body">
                <div class="diag-row">
                  <span class="diag-label">Ping Latency</span>
                  <span class="diag-val">{{ diagnostics()!.tmdbLatencyMs }} ms</span>
                </div>
                <div class="diag-row">
                  <span class="diag-label">Endpoint Status</span>
                  <span class="diag-val">{{ diagnostics()!.tmdbStatus === 'Healthy' ? 'OPERATIONAL' : 'FAULT' }}</span>
                </div>
              </div>
            </div>

            <div class="health-card crosshair-bracket">
              <div class="provider-header">
                <span class="provider-name">Open Movie Database (OMDb)</span>
                <span class="health-badge" [class.healthy]="diagnostics()!.omdbStatus === 'Healthy'" [class.unhealthy]="diagnostics()!.omdbStatus !== 'Healthy'">
                  {{ diagnostics()!.omdbStatus }}
                </span>
              </div>
              <div class="provider-body">
                <div class="diag-row">
                  <span class="diag-label">Ping Latency</span>
                  <span class="diag-val">{{ diagnostics()!.omdbLatencyMs }} ms</span>
                </div>
                <div class="diag-row">
                  <span class="diag-label">Endpoint Status</span>
                  <span class="diag-val">{{ diagnostics()!.omdbStatus === 'Healthy' ? 'OPERATIONAL' : 'FAULT' }}</span>
                </div>
              </div>
            </div>

            <div style="text-align: right; margin-top: 16px;">
              <button class="btn btn-record" (click)="loadProviders()" [disabled]="actionPending()">
                Refresh Connection Health
              </button>
            </div>

          </div>
        </div>

        <!-- USER MANAGEMENT TAB -->
        <div *ngSwitchCase="'users'" class="tab-pane animate-fade-in">
          <div class="card crosshair-bracket" style="max-width: 100%;">
            <h2 class="card-title" style="margin-bottom: 16px;">Registered Users</h2>
            
            <div class="search-bar-container">
              <input 
                type="text" 
                class="hud-input" 
                placeholder="SEARCH BY USERNAME OR EMAIL..." 
                [value]="searchTerm()"
                (input)="onSearchInput($event)"
              />
            </div>

            <div class="table-container">
              <table class="hud-table">
                <thead>
                  <tr>
                    <th>Username</th>
                    <th>Email Address</th>
                    <th>Role</th>
                    <th style="text-align: right;">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let user of filteredUsers()">
                    <td>{{ user.username }}</td>
                    <td>{{ user.email }}</td>
                    <td>
                      <span class="role-badge" [class.admin]="user.role === 'Admin'">
                        {{ user.role }}
                      </span>
                    </td>
                    <td style="text-align: right;">
                      <button 
                        *ngIf="user.role !== 'Admin'"
                        class="btn btn-mini btn-sepia"
                        (click)="promoteUser(user.id)"
                        [disabled]="actionPending()">
                        Promote to Admin
                      </button>
                      <span *ngIf="user.role === 'Admin'" class="action-muted">No Actions Available</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- DIAGNOSTIC LOGS TAB -->
        <div *ngSwitchCase="'logs'" class="tab-pane animate-fade-in">
          <div class="console-card crosshair-bracket">
            <div class="console-header">
              <span class="console-title">RECENT WARNINGS & ERRORS</span>
              <button class="btn btn-mini btn-muted" (click)="loadLogs()" [disabled]="actionPending()">
                Refresh Logs
              </button>
            </div>
            <div class="console-body">
              <div class="console-placeholder" *ngIf="!logs().length">
                No recent warnings or errors logged in buffer. System is running cleanly.
              </div>
              <div class="log-entry" *ngFor="let log of logs()">
                <span class="log-time">[{{ log.timestamp | date:'HH:mm:ss' }}]</span>
                <span class="log-level" [class.error]="log.level === 'Error' || log.level === 'Critical'">{{ log.level }}</span>
                <span class="log-category">[{{ log.category }}]</span>
                <span class="log-msg">{{ log.message }}</span>
                <pre class="log-stack" *ngIf="log.exception">{{ log.exception }}</pre>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  `,
  styles: [`
    .admin-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 32px 24px;
      color: var(--text-primary);
      min-height: 80vh;
    }

    .header-section {
      margin-bottom: 32px;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 20px;
    }

    .page-title {
      font-size: 2.2rem;
      font-weight: 700;
      letter-spacing: -0.5px;
      display: inline-flex;
      align-items: center;
      gap: 12px;
      margin: 0 0 8px 0;
    }

    .title-icon {
      width: 32px;
      height: 32px;
      color: var(--accent-silver);
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 0.95rem;
      margin: 0;
    }

    /* Tabs Navigation */
    .tabs-nav {
      display: flex;
      gap: 8px;
      margin-bottom: 32px;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 1px;
    }

    .tabs-nav button {
      background: none;
      border: none;
      border-bottom: 2px solid transparent;
      color: var(--text-secondary);
      padding: 12px 20px;
      font-weight: 600;
      font-size: 0.9rem;
      cursor: pointer;
      transition: all 0.2s ease;
      text-transform: uppercase;
      letter-spacing: 0.5px;

      &:hover {
        color: var(--text-primary);
        border-bottom-color: rgba(255, 255, 255, 0.4);
      }

      &.active {
        color: var(--text-primary);
        border-bottom-color: var(--accent-sepia);
      }
    }

    /* Banners */
    .alert-banner {
      display: flex;
      align-items: center;
      padding: 14px 20px;
      margin-bottom: 24px;
      font-size: 0.9rem;
      border-radius: 4px;
      position: relative;

      &.error {
        color: var(--accent-record);
        border: 1px solid rgba(229, 9, 20, 0.4);
        background: rgba(229, 9, 20, 0.05);
      }

      &.success {
        color: var(--accent-emerald);
        border: 1px solid rgba(16, 185, 129, 0.4);
        background: rgba(16, 185, 129, 0.05);
      }
    }

    .banner-title {
      font-weight: 700;
      margin-right: 12px;
      letter-spacing: 0.5px;
      text-transform: uppercase;
    }

    .banner-text {
      flex: 1;
    }

    .dismiss-btn {
      background: none;
      border: none;
      color: inherit;
      font-size: 1.5rem;
      cursor: pointer;
      line-height: 1;
      opacity: 0.6;
      transition: opacity 0.2s;
      padding: 0 4px;
      &:hover {
        opacity: 1;
      }
    }

    /* Brackets UI pattern */
    .crosshair-bracket {
      position: relative;
      border: 1px solid var(--border-color);
      background: var(--surface-elevated);
      padding: 24px;
      border-radius: 4px;

      &::before, &::after {
        content: '';
        position: absolute;
        width: 10px;
        height: 10px;
        border-color: var(--accent-silver);
        border-style: solid;
        pointer-events: none;
      }

      &::before {
        top: -1px;
        left: -1px;
        border-width: 2px 0 0 2px;
      }

      &::after {
        bottom: -1px;
        right: -1px;
        border-width: 0 2px 2px 0;
      }
    }

    /* Content Layout */
    .pane-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(450px, 1fr));
      gap: 24px;
    }

    .card-title {
      font-size: 1.25rem;
      font-weight: 700;
      letter-spacing: 0.5px;
      text-transform: uppercase;
      margin: 0 0 20px 0;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 10px;
      color: var(--accent-silver);
    }

    /* Statistics */
    .stats-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
      margin-bottom: 24px;
    }

    .stat-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px dashed rgba(255, 255, 255, 0.05);
      padding-bottom: 8px;
    }

    .stat-label {
      color: var(--text-secondary);
      font-size: 0.9rem;
    }

    .stat-val {
      font-weight: 700;
      font-family: monospace;
      font-size: 1.05rem;

      &.highlight {
        color: var(--accent-sepia);
      }

      &.completed {
        color: var(--accent-emerald);
      }

      &.failed {
        color: var(--accent-record);
        opacity: 0.8;
      }

      &.critical {
        color: var(--accent-record);
        font-weight: 900;
      }
    }

    /* Actions & Buttons */
    .card-actions {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
    }

    .btn {
      padding: 10px 18px;
      font-size: 0.85rem;
      font-weight: 700;
      text-transform: uppercase;
      border: 1px solid transparent;
      border-radius: 4px;
      cursor: pointer;
      transition: all 0.2s ease;
      letter-spacing: 0.5px;

      &:disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }
    }

    .btn-record {
      background: var(--accent-record);
      color: #fff;
      &:hover:not(:disabled) {
        background: #b5070f;
      }
    }

    .btn-sepia {
      background: var(--accent-sepia);
      color: #000;
      &:hover:not(:disabled) {
        background: #cba34f;
      }
    }

    .btn-muted {
      background: transparent;
      border-color: var(--border-color);
      color: var(--text-secondary);
      &:hover:not(:disabled) {
        background: rgba(255, 255, 255, 0.05);
        color: var(--text-primary);
      }
    }

    .btn-mini {
      padding: 6px 12px;
      font-size: 0.75rem;
    }

    /* Loading */
    .loading-overlay {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 48px;
      color: var(--text-secondary);
      font-size: 0.95rem;
    }

    .spinner {
      width: 32px;
      height: 32px;
      border: 3px solid rgba(255, 255, 255, 0.1);
      border-radius: 50%;
      border-top-color: var(--accent-sepia);
      animation: spin 1s ease-in-out infinite;
      margin-bottom: 12px;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    /* Providers Health */
    .health-container {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .health-card {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .provider-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 10px;
    }

    .provider-name {
      font-weight: 700;
      font-size: 1.1rem;
      letter-spacing: 0.5px;
    }

    .health-badge {
      font-size: 0.75rem;
      font-weight: 800;
      text-transform: uppercase;
      padding: 4px 8px;
      border-radius: 3px;
      letter-spacing: 0.5px;

      &.healthy {
        background: rgba(16, 185, 129, 0.1);
        border: 1px solid var(--accent-emerald);
        color: var(--accent-emerald);
      }

      &.unhealthy {
        background: rgba(229, 9, 20, 0.1);
        border: 1px solid var(--accent-record);
        color: var(--accent-record);
      }
    }

    .provider-body {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .diag-row {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .diag-label {
      color: var(--text-secondary);
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .diag-val {
      font-weight: 700;
      font-family: monospace;
      font-size: 1rem;
    }

    /* User Management Table */
    .table-container {
      overflow-x: auto;
    }

    .hud-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.9rem;
      text-align: left;

      th, td {
        padding: 12px 16px;
        border-bottom: 1px solid var(--border-color);
      }

      th {
        font-weight: 700;
        text-transform: uppercase;
        color: var(--accent-silver);
        font-size: 0.8rem;
        letter-spacing: 0.5px;
      }

      tbody tr:hover {
        background: rgba(255, 255, 255, 0.02);
      }
    }

    .role-badge {
      font-size: 0.75rem;
      font-weight: 700;
      padding: 2px 6px;
      border-radius: 3px;
      border: 1px solid rgba(255, 255, 255, 0.2);
      color: var(--text-secondary);

      &.admin {
        border-color: var(--accent-sepia);
        color: var(--accent-sepia);
        background: rgba(226, 186, 100, 0.05);
      }
    }

    .action-muted {
      color: var(--text-muted);
      font-size: 0.8rem;
    }

    /* Diagnostic Console Logs */
    .console-card {
      display: flex;
      flex-direction: column;
      gap: 16px;
      background: #000 !important;
      border-color: rgba(255, 255, 255, 0.15) !important;
    }

    .console-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px solid rgba(255, 255, 255, 0.15);
      padding-bottom: 8px;
    }

    .console-title {
      font-family: monospace;
      font-weight: 700;
      font-size: 0.85rem;
      color: var(--text-secondary);
      letter-spacing: 0.5px;
    }

    .console-body {
      background: #050505;
      border: 1px solid rgba(255, 255, 255, 0.05);
      padding: 16px;
      font-family: monospace;
      font-size: 0.8rem;
      line-height: 1.5;
      height: 400px;
      overflow-y: auto;
      border-radius: 3px;
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .console-placeholder {
      color: var(--text-muted);
      text-align: center;
      padding-top: 150px;
    }

    .log-entry {
      border-bottom: 1px solid rgba(255, 255, 255, 0.02);
      padding-bottom: 6px;
      word-break: break-all;
    }

    .log-time {
      color: var(--text-muted);
      margin-right: 8px;
    }

    .log-level {
      color: var(--accent-sepia);
      font-weight: 700;
      margin-right: 8px;
      text-transform: uppercase;

      &.error {
        color: var(--accent-record);
      }
    }

    .log-category {
      color: #6a8cff;
      margin-right: 8px;
    }

    .log-msg {
      color: var(--text-primary);
    }

    .search-bar-container {
      margin-bottom: 20px;
    }

    .hud-input {
      width: 100%;
      max-width: 400px;
      background: var(--bg-tertiary);
      border: 1px solid var(--border-color);
      color: var(--text-primary);
      padding: 10px 14px;
      font-size: 0.85rem;
      font-family: monospace;
      text-transform: uppercase;
      border-radius: 4px;
      outline: none;
      letter-spacing: 0.5px;
      transition: all 0.2s ease;

      &:focus {
        border-color: var(--accent-sepia);
        box-shadow: 0 0 8px rgba(226, 186, 100, 0.1);
      }
    }

    .log-stack {
      margin: 6px 0 0 0;
      background: rgba(255, 9, 20, 0.05);
      border-left: 2px solid var(--accent-record);
      padding: 8px;
      color: #ff9b9f;
      font-size: 0.75rem;
      white-space: pre-wrap;
    }

    /* Animations */
    .animate-fade-in {
      animation: fadeIn 0.3s ease-out forwards;
    }

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(4px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class AdminPanelComponent implements OnInit {
  public activeTab = signal<AdminTab>('database');
  public isLoading = signal<boolean>(false);
  public actionPending = signal<boolean>(false);
  public errorMessage = signal<string | null>(null);
  public successMessage = signal<string | null>(null);

  // States
  public stats = signal<DatabaseStatsDto | null>(null);
  public diagnostics = signal<ProviderDiagnosticsDto | null>(null);
  public users = signal<UserDto[]>([]);
  public logs = signal<LogEntryDto[]>([]);

  // Search
  public searchTerm = signal<string>('');
  public filteredUsers = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) {
      return this.users();
    }
    return this.users().filter(u => 
      u.username.toLowerCase().includes(term) || 
      u.email.toLowerCase().includes(term)
    );
  });

  public onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
  }

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadTabInfo();
  }

  public setTab(tab: AdminTab): void {
    this.activeTab.set(tab);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.loadTabInfo();
  }

  private loadTabInfo(): void {
    const tab = this.activeTab();
    this.isLoading.set(true);

    if (tab === 'database') {
      this.adminService.apiAdminDiagnosticsDatabaseGet().subscribe({
        next: (data) => {
          this.stats.set(data);
          this.isLoading.set(false);
        },
        error: (err) => this.handleError('Failed to load database stats', err)
      });
    } else if (tab === 'providers') {
      this.loadProviders();
    } else if (tab === 'users') {
      this.adminService.apiAdminUsersGet().subscribe({
        next: (data) => {
          this.users.set(data);
          this.isLoading.set(false);
        },
        error: (err) => this.handleError('Failed to load users list', err)
      });
    } else if (tab === 'logs') {
      this.loadLogs();
    }
  }

  public loadProviders(): void {
    this.isLoading.set(true);
    this.adminService.apiAdminDiagnosticsProvidersGet().subscribe({
      next: (data) => {
        this.diagnostics.set(data);
        this.isLoading.set(false);
      },
      error: (err) => this.handleError('Failed to test API connections', err)
    });
  }

  public loadLogs(): void {
    this.isLoading.set(true);
    this.adminService.apiAdminDiagnosticsLogsGet().subscribe({
      next: (data) => {
        this.logs.set(data);
        this.isLoading.set(false);
      },
      error: (err) => this.handleError('Failed to load system logs', err)
    });
  }

  public runRetry(resetPermanentlyFailed: boolean): void {
    console.log('[AdminPanel] runRetry clicked', { resetPermanentlyFailed });
    this.actionPending.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.adminService.apiAdminEnrichRetryFailedPost(resetPermanentlyFailed, 50).subscribe({
      next: (recoveredCount) => {
        console.log('[AdminPanel] runRetry success', { recoveredCount });
        this.actionPending.set(false);
        this.successMessage.set(`Manual enrichment retry complete. Recovered ${recoveredCount} movies.`);
        this.loadTabInfo(); // Refresh stats
      },
      error: (err) => {
        console.error('[AdminPanel] runRetry error', err);
        this.actionPending.set(false);
        this.handleError('Failed to execute enrichment retry', err);
      }
    });
  }

  public purgeOrphans(): void {
    console.log('[AdminPanel] purgeOrphans clicked');
    if (!confirm('Are you sure you want to delete orphaned actors, directors, and genres? This action is permanent.')) {
      console.log('[AdminPanel] purgeOrphans cancelled by user');
      return;
    }

    console.log('[AdminPanel] purgeOrphans confirmed, sending request');
    this.actionPending.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.adminService.apiAdminMaintenancePurgeOrphansPost().subscribe({
      next: (result: PurgeOrphanResultDto) => {
        console.log('[AdminPanel] purgeOrphans success', result);
        this.actionPending.set(false);
        this.successMessage.set(`Database cleanup successful. Purged: ${result.purgedGenres} genres, ${result.purgedDirectors} directors, and ${result.purgedActors} actors.`);
        this.loadTabInfo(); // Refresh stats
      },
      error: (err) => {
        console.error('[AdminPanel] purgeOrphans error', err);
        this.actionPending.set(false);
        this.handleError('Failed to clean database', err);
      }
    });
  }

  public clearCache(): void {
    console.log('[AdminPanel] clearCache clicked');
    this.actionPending.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.adminService.apiAdminMaintenanceClearCachePost().subscribe({
      next: () => {
        console.log('[AdminPanel] clearCache success');
        this.actionPending.set(false);
        this.successMessage.set('System recommendations and metadata cache successfully cleared.');
      },
      error: (err) => {
        console.error('[AdminPanel] clearCache error', err);
        this.actionPending.set(false);
        this.handleError('Failed to clear system cache', err);
      }
    });
  }

  public promoteUser(userId: string): void {
    console.log('[AdminPanel] promoteUser clicked', { userId });
    if (!confirm('Are you sure you want to promote this user to Admin?')) {
      console.log('[AdminPanel] promoteUser cancelled by user');
      return;
    }

    console.log('[AdminPanel] promoteUser confirmed, sending request');
    this.actionPending.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.adminService.apiAdminUsersUserIdPromotePost(userId).subscribe({
      next: () => {
        console.log('[AdminPanel] promoteUser success');
        this.actionPending.set(false);
        this.successMessage.set('User successfully promoted to Admin.');
        this.loadTabInfo(); // Refresh users table
      },
      error: (err) => {
        console.error('[AdminPanel] promoteUser error', err);
        this.actionPending.set(false);
        this.handleError('Failed to promote user', err);
      }
    });
  }

  private handleError(message: string, error: any): void {
    this.isLoading.set(false);
    console.error(message, error);
    const detail = error?.error?.message ?? error?.message ?? 'Unknown connection error';
    this.errorMessage.set(`${message}: ${detail}`);
  }
}
