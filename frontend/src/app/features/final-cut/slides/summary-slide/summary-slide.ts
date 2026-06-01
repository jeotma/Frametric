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
    <div class="slide-content summary-wrapper">
      <div class="summary-card" id="final-cut-card">
        <div class="card-header">
          <div class="logo">Frametric</div>
          <div class="year">THE {{ username | uppercase }}'S CUT {{ year }}</div>
        </div>

        <div class="card-body">
          <div class="data-grid">
            <!-- Column 1 -->
            <div class="data-col">
              <div class="stat-block">
                <span class="lbl">Movies</span>
                <span class="val">{{ data.totalWatches }}</span>
              </div>
              <div class="list-block">
                <span class="list-title">Top Genres</span>
                <ol>
                  <li *ngFor="let g of data.topGenres.slice(0, 5)">{{ g.genreName }}</li>
                </ol>
              </div>
            </div>

            <!-- Column 2 -->
            <div class="data-col">
              <div class="stat-block">
                <span class="lbl">Unique</span>
                <span class="val">{{ data.uniqueMoviesCount }}</span>
              </div>
              <div class="list-block">
                <span class="list-title">Top Directors</span>
                <ol>
                  <li *ngFor="let d of data.topDirectors.slice(0, 5)">{{ d.directorName }}</li>
                </ol>
              </div>
            </div>

            <!-- Column 3 -->
            <div class="data-col">
              <div class="stat-block">
                <span class="lbl">Hours</span>
                <span class="val">{{ (data.totalWatchtimeMinutes / 60) | number:'1.0-0' }}</span>
              </div>
              <div class="list-block">
                <span class="list-title">Top Actors</span>
                <ol>
                  <li *ngFor="let a of data.topActors.slice(0, 5)">{{ a.actorName }}</li>
                </ol>
              </div>
            </div>
          </div>
        </div>

        <div class="card-footer">
          <div class="user-handle">@{{ username }}</div>
          <div class="watermark-container">
            <div class="app-watermark">frametric.app</div>
            <div class="creator-watermark">jesusoteromartinez@outlook.com</div>
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
    </div>
  `,
  styles: [`
    .summary-wrapper {
      background: radial-gradient(circle at center, rgba(139, 92, 246, 0.1) 0%, transparent 100%);
    }
    
    /* VERY strict CSS for html2canvas compatibility */
    .summary-card {
      background-color: #111116; /* Solid color, no rgba/blur for better html2canvas */
      border: 1px solid #333;
      border-radius: 16px;
      padding: 36px;
      width: 100%;
      max-width: 660px;
      color: #fff;
      font-family: 'Inter', sans-serif;
      box-shadow: 0 20px 40px rgba(0,0,0,0.5);
      margin-bottom: 40px;
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;
      border-bottom: 1px solid #333;
      padding-bottom: 16px;
    }

    .logo {
      font-weight: 800;
      font-size: 1.3rem;
      letter-spacing: -0.5px;
    }

    .year {
      background: #8b5cf6;
      color: #fff;
      padding: 4px 10px;
      border-radius: 4px;
      font-weight: 700;
      font-size: 0.9rem;
    }

    .data-grid {
      display: grid;
      grid-template-columns: 1fr 1fr 1fr;
      gap: 16px;
      margin-bottom: 32px;
      width: 100%;
      justify-items: center; /* Center the entire column block */
    }

    .data-col {
      display: flex;
      flex-direction: column;
      align-items: flex-start; /* Ensures stat and list align to the SAME left edge */
      gap: 36px; /* Space between stat and list */
      width: max-content;
    }

    .stat-block, .list-block {
      display: flex;
      flex-direction: column;
      text-align: left;
    }

    .stat-block {
      gap: 4px;
    }

    .lbl {
      color: #aaa;
      font-size: 0.85rem;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    .val {
      font-size: 2rem;
      font-weight: 800;
      color: #fff;
    }

    .list-title {
      display: block;
      color: #8b5cf6;
      font-weight: 700;
      margin-bottom: 12px;
      font-size: 1rem;
      text-transform: uppercase;
    }

    ol {
      margin: 0;
      padding: 0;
      list-style-type: none;
      color: #ddd;
      font-size: 1rem;
    }

    li {
      margin-bottom: 8px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      max-width: 100%;
    }

    .card-footer {
      text-align: center;
      color: #666;
      border-top: 1px solid #333;
      padding-top: 24px;
      padding-bottom: 24px;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }
    
    .user-handle {
      color: var(--accent-purple);
      font-size: 1.1rem;
      font-weight: 800;
      letter-spacing: 0.5px;
    }

    .watermark-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 4px;
      margin-top: 4px;
      width: 100%;
    }

    .app-watermark {
      color: rgba(255, 255, 255, 0.4);
      font-size: 0.8rem;
      font-weight: 600;
      letter-spacing: 1px;
    }
    
    .creator-watermark {
      color: rgba(255, 255, 255, 0.2);
      font-size: 0.65rem;
      font-weight: 500;
    }

    .action-buttons {
      display: flex;
      flex-direction: column;
      gap: 16px;
      align-items: center;
      position: relative;
      z-index: 100;
      pointer-events: auto; /* Enable clicks for all buttons */
    }

    .share-btn {
      background: linear-gradient(135deg, var(--accent-purple), var(--accent-pink));
      color: white;
      border: none;
      padding: 16px 40px;
      border-radius: 99px;
      font-size: 1.1rem;
      font-weight: 700;
      cursor: pointer;
      box-shadow: 0 4px 15px rgba(139, 92, 246, 0.4);
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
  @Input() year!: number;
  @Input() username!: string;

  public isGenerating = signal<boolean>(false);

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
