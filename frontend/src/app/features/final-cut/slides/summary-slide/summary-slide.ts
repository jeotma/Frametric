import { Component, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';
import html2canvas from 'html2canvas';

@Component({
  selector: 'app-summary-slide',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="slide-content slide-bg-sepia">
      <h2 class="slide-title" style="font-size: 2.2rem; margin-bottom: 8px;">Act IV<span style="color: var(--text-muted)"> - </span>Scene 21<span style="color: var(--text-muted)"> - </span>The Closing Credits</h2>
      <p class="slide-subtitle" style="margin-bottom: 12px;">And that's a wrap on {{ year === 'global' ? 'a lifetime of cinema' : year }}.</p>
      <p class="slide-explainer" style="margin-bottom: 16px; font-size: 0.85rem;">The projector cools down, the iris fades to black, and the credits roll on your cinematic journey.</p>

      <div class="summary-card" id="final-cut-card">
        <!-- Decorative Brackets -->
        <div class="v-bracket v-tl"></div>
        <div class="v-bracket v-tr"></div>
        <div class="v-bracket v-bl"></div>
        <div class="v-bracket v-br"></div>

        <div class="card-header">
          <div class="logo">
            <svg class="clapper-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="width: 20px; height: 20px; margin-right: 6px; color: #d4d4d8;"><path d="M20 21H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2z"></path><line x1="2" y1="9" x2="22" y2="9"></line><path d="M4 5l4 4"></path><path d="M10 5l4 4"></path><path d="M16 5l4 4"></path></svg>
            Frametric
          </div>
          <div class="year">THE {{ username | uppercase }}'S CUT {{ year === 'global' ? 'ALL-TIME' : year }}</div>
        </div>

        <div class="card-body">
          <!-- Global Stats Row -->
          <div class="global-stats-row">
            <div class="global-stat stat-emerald">
              <span class="lbl" style="color: #10b981;"><span style="color: #10b981; margin-right: 4px;">★</span> Movies</span>
              <span class="val highlight-val" style="font-family: var(--font-mono); color: #10b981;">{{ data.totalWatches }}</span>
            </div>
            <div class="global-stat stat-sepia">
              <span class="lbl" style="color: #e2ba64;"><span style="color: #e2ba64; margin-right: 4px;">✦</span> Unique Films</span>
              <span class="val highlight-val" style="font-family: var(--font-mono); color: #e2ba64;">{{ data.uniqueMoviesCount }}</span>
            </div>
            <div class="global-stat stat-record">
              <span class="lbl" style="color: #e50914;"><span style="color: #e50914; margin-right: 4px;">●</span> Hours</span>
              <span class="val highlight-val" style="font-family: var(--font-mono); color: #e50914;">{{ (data.totalWatchtimeMinutes / 60) | number:'1.0-0' }}</span>
            </div>
          </div>

          <div class="data-grid">
            <!-- Column 1 (Emerald) -->
            <div class="data-col col-emerald">
              <div class="list-block">
                <span class="list-title" style="color: #10b981; border-bottom: 1px solid rgba(16, 185, 129, 0.3); padding-bottom: 6px;">Top Genres</span>
                <ol class="highlighted-list">
                  <li *ngFor="let g of data.topGenres.slice(0, 5)">
                    <span class="list-dot" style="color: #10b981; font-size: 0.7em;">■</span> {{ g.genreName }}
                  </li>
                </ol>
              </div>
            </div>

            <!-- Column 2 (Sepia) -->
            <div class="data-col col-sepia main-col">
              <div class="list-block">
                <span class="list-title" style="color: #e2ba64; border-bottom: 1px solid rgba(226, 186, 100, 0.3); padding-bottom: 6px;">Top Directors</span>
                <ol class="highlighted-list">
                  <li *ngFor="let d of data.topDirectors.slice(0, 5)">
                    <span class="list-dot" style="color: #e2ba64; font-size: 0.7em;">■</span>
                    {{ d.directorName }}
                  </li>
                </ol>
              </div>
            </div>

            <!-- Column 3 (Record) -->
            <div class="data-col col-record">
              <div class="list-block">
                <span class="list-title" style="color: #e50914; border-bottom: 1px solid rgba(229, 9, 20, 0.3); padding-bottom: 6px;">Top Actors</span>
                <ol class="highlighted-list">
                  <li *ngFor="let a of data.topActors.slice(0, 5)">
                    <span class="list-dot" style="color: #e50914; font-size: 0.7em;">■</span> {{ a.actorName }}
                  </li>
                </ol>
              </div>
            </div>
          </div>

          <div class="extra-stats" *ngIf="topDecade">
            <div class="mini-stat">
              <span class="mini-lbl">Top Decade</span>
              <span class="mini-val">{{ topDecade }}s</span>
            </div>
            <div class="mini-separator"></div>
            <div class="mini-stat" *ngIf="peakMonth">
              <span class="mini-lbl">Peak Month</span>
              <span class="mini-val">{{ peakMonth }}</span>
            </div>
          </div>
        </div>

        <div class="card-footer">
          <div class="user-handle">@{{ username }}</div>
          <div class="watermark-container">
            <span class="watermark-text">frametric.app</span>
            <span class="watermark-dot">•</span>
            <a href="https://www.linkedin.com/in/jesus-otero-dev" target="_blank" class="watermark-link">
              <svg class="linkedin-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                <path d="M19 0h-14c-2.761 0-5 2.239-5 5v14c0 2.761 2.239 5 5 5h14c2.762 0 5-2.239 5-5v-14c0-2.761-2.238-5-5-5zm-11 19h-3v-11h3v11zm-1.5-12.268c-.966 0-1.75-.779-1.75-1.75s.784-1.75 1.75-1.75 1.75.779 1.75 1.75-.784 1.75-1.75 1.75zm13.5 12.268h-3v-5.604c0-3.368-4-3.113-4 0v5.604h-3v-11h3v1.765c1.396-2.586 7-2.777 7 2.476v6.759z"/>
              </svg>
              jesus-otero-dev
            </a>
          </div>
        </div>
      </div>

      <div class="action-buttons">
        <button class="share-btn" (click)="share()" [disabled]="isGenerating()">
          {{ isGenerating() ? 'Generating...' : 'Share Your Final Cut' }}
        </button>
        <button routerLink="/" class="exit-btn">
          Finish & Return
        </button>
      </div>

      <p class="exit-note">Press ESC or click the exit button to return.</p>
      <div class="timecode">TC 02:30:00:00</div>
    </div>
  `,
  styles: [`
    .final-note {
      text-align: center;
      font-size: 1.5rem;
      font-weight: 800;
      color: var(--text-primary);
      margin-bottom: 16px;
    }
    .fn-sub {
      display: block;
      font-size: 0.9rem;
      font-weight: 400;
      color: var(--text-muted);
      margin-top: 8px;
    }
    
    .exit-note {
      font-size: 0.8rem;
      color: rgba(255,255,255,0.3);
      animation: pulse 2s infinite;
      margin-top: 8px;
    }
    @keyframes pulse {
      0%, 100% { opacity: 0.4; }
      50% { opacity: 1; }
    }
    
    .summary-card {
      background-color: #0a0a0c;
      border: 1px solid #222;
      border-radius: 12px;
      padding: 16px 24px;
      width: 100%;
      max-width: 600px;
      color: #fff;
      font-family: 'Inter', sans-serif;
      box-shadow: 0 24px 50px rgba(0,0,0,0.8);
      margin-bottom: 12px;
      position: relative;
    }

    .v-bracket {
      position: absolute;
      width: 24px;
      height: 24px;
      border: 2px solid #a3a3a3;
      pointer-events: none;
    }
    .v-tl { top: 8px; left: 8px; border-right: none; border-bottom: none; }
    .v-tr { top: 8px; right: 8px; border-left: none; border-bottom: none; }
    .v-bl { bottom: 8px; left: 8px; border-right: none; border-top: none; }
    .v-br { bottom: 8px; right: 8px; border-left: none; border-top: none; }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
      border-bottom: 1px solid #333;
      padding-bottom: 12px;
    }

    .logo {
      display: flex;
      align-items: center;
      font-weight: 900;
      font-size: 1.4rem;
      letter-spacing: -0.5px;
    }

    .year {
      background: #e2ba64;
      color: #000;
      padding: 4px 12px;
      border-radius: 4px;
      font-weight: 800;
      font-size: 0.9rem;
      letter-spacing: 0.5px;
    }

    .card-body {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .data-grid {
      display: grid;
      grid-template-columns: 1fr 1.2fr 1fr;
      gap: 16px;
      margin-bottom: 12px;
      width: 100%;
    }

    .global-stats-row {
      display: flex;
      justify-content: space-around;
      width: 100%;
      margin-bottom: 16px;
      padding-bottom: 16px;
      border-bottom: 1px dashed rgba(255,255,255,0.1);
    }

    .global-stat {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      gap: 6px;
    }

    .data-col {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      gap: 12px;
      border-radius: 8px;
      padding: 12px 8px;
      position: relative;
    }

    .col-emerald {
      border: 1px solid rgba(16, 185, 129, 0.1);
      background: radial-gradient(circle at top, rgba(16, 185, 129, 0.06) 0%, rgba(0,0,0,0) 70%);
    }
    .col-sepia {
      border: 1px solid rgba(226, 186, 100, 0.1);
      background: radial-gradient(circle at top, rgba(226, 186, 100, 0.06) 0%, rgba(0,0,0,0) 70%);
    }
    .col-record {
      border: 1px solid rgba(229, 9, 20, 0.1);
      background: radial-gradient(circle at top, rgba(229, 9, 20, 0.06) 0%, rgba(0,0,0,0) 70%);
    }

    .main-col {
      transform: scale(1.02);
      z-index: 2;
      box-shadow: 0 10px 30px rgba(0,0,0,0.5);
    }

    .stat-block, .list-block {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      width: 100%;
    }

    .stat-block {
      gap: 4px;
    }

    .lbl {
      color: #a3a3a3;
      font-size: 0.85rem;
      text-transform: uppercase;
      letter-spacing: 1.5px;
      font-weight: 600;
      display: flex;
      align-items: center;
    }

    .val {
      font-size: 1.6rem;
      font-weight: 800;
      color: #fff;
    }
    
    .highlight-val {
      font-size: 2.2rem;
    }
    .col-emerald .highlight-val { text-shadow: 0 0 15px rgba(16, 185, 129, 0.3); }
    .col-sepia .highlight-val { text-shadow: 0 0 15px rgba(226, 186, 100, 0.3); }
    .col-record .highlight-val { text-shadow: 0 0 15px rgba(229, 9, 20, 0.3); }

    .list-title {
      display: block;
      color: #d4d4d8;
      font-weight: 800;
      margin-bottom: 16px;
      font-size: 0.9rem;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    ol {
      margin: 0;
      padding: 0;
      list-style-type: none;
      color: #d4d4d8;
      font-size: 0.85rem;
      display: inline-flex;
      flex-direction: column;
      align-items: flex-start;
      text-align: left;
    }

    .highlighted-list {
      color: #fff;
      font-weight: 500;
    }

    li {
      margin-bottom: 6px;
      text-align: left;
      display: flex;
      align-items: center;
      justify-content: flex-start;
      gap: 8px;
    }

    .extra-stats {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 24px;
      padding-top: 12px;
      border-top: 1px dashed #333;
      width: 80%;
    }

    .mini-stat {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .mini-lbl {
      font-size: 0.7rem;
      color: #737373;
      text-transform: uppercase;
      letter-spacing: 1px;
      margin-bottom: 4px;
    }

    .mini-val {
      font-family: var(--font-mono);
      font-size: 1.1rem;
      font-weight: 700;
      color: #d4d4d8;
    }

    .mini-separator {
      width: 1px;
      height: 24px;
      background-color: #333;
    }

    .card-footer {
      border-top: 1px solid #333;
      padding-top: 12px;
      margin-top: 8px;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
    }
    
    .user-handle {
      color: #e2ba64;
      font-size: 1.2rem;
      font-weight: 800;
      letter-spacing: 0.5px;
    }

    .watermark-container {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      width: 100%;
    }

    .watermark-text {
      color: #737373;
      font-size: 0.75rem;
      font-weight: 500;
      letter-spacing: 0.5px;
      text-transform: lowercase;
    }

    .watermark-link {
      color: #737373;
      font-size: 0.75rem;
      font-weight: 500;
      letter-spacing: 0.5px;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      gap: 4px;
    }
    
    .linkedin-icon {
      width: 12px;
      height: 12px;
      fill: currentColor;
    }

    .watermark-dot {
      color: #333;
      font-size: 0.6rem;
    }

    .action-buttons {
      display: flex;
      flex-direction: row;
      gap: 24px;
      align-items: center;
      position: relative;
      z-index: 100;
      pointer-events: auto; /* Enable clicks for all buttons */
      margin-bottom: 16px;
    }

    .share-btn {
      background: linear-gradient(135deg, var(--accent-sepia), #b89243);
      color: #000;
      border: none;
      padding: 16px 40px;
      border-radius: 99px;
      font-size: 1.1rem;
      font-weight: 800;
      cursor: pointer;
      box-shadow: 0 4px 15px rgba(226, 186, 100, 0.3);
      transition: transform 0.2s;
    }

    .share-btn:hover:not(:disabled) {
      transform: scale(1.05);
    }
    
    .share-btn:disabled {
      opacity: 0.7;
      cursor: wait;
    }

    .exit-btn {
      background: transparent;
      color: var(--text-secondary);
      border: 1px solid var(--border-color);
      padding: 10px 24px;
      border-radius: 99px;
      font-size: 0.95rem;
      font-family: var(--font-display);
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }

    .exit-btn:hover {
      background: rgba(255, 255, 255, 0.05);
      color: var(--text-primary);
      border-color: rgba(255, 255, 255, 0.2);
    }
  `]
})
export class SummarySlideComponent {
  @Input({ required: true }) data!: WrappedSummaryDto;
  @Input() year!: number | 'global';
  @Input() username!: string;

  public isGenerating = signal<boolean>(false);

  get topDecade() {
    if (!this.data?.decadeBreakdown?.length) return null;
    return [...this.data.decadeBreakdown].sort((a, b) => b.count - a.count)[0].decade;
  }

  get peakMonth() {
    if (!this.data?.monthlyActivity?.length) return null;
    const peak = [...this.data.monthlyActivity].sort((a, b) => b.count - a.count)[0];
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    return months[peak.month - 1] || peak.month;
  }

  public async share() {
    this.isGenerating.set(true);
    try {
      const element = document.getElementById('final-cut-card');
      if (!element) return;

      // Force fixed width for a pristine snapshot
      const originalWidth = element.style.width;
      const originalMaxWidth = element.style.maxWidth;
      element.style.width = '600px';
      element.style.maxWidth = '600px';
      
      // Wait a tick for the browser to apply layout changes
      await new Promise(resolve => setTimeout(resolve, 100));

      const canvas = await html2canvas(element, {
        scale: 2, // High resolution
        backgroundColor: '#111116',
        useCORS: true
      });
      
      // Restore styles
      element.style.width = originalWidth;
      element.style.maxWidth = originalMaxWidth;

      const image = canvas.toDataURL('image/png');
      
      // Trigger download
      const link = document.createElement('a');
      link.href = image;
      link.download = 'frametric-final-cut-' + this.year + '.png';
      link.click();
    } catch (error) {
      console.error('Error generating image', error);
      alert('Could not generate the image. Please try again.');
    } finally {
      this.isGenerating.set(false);
    }
  }
}
