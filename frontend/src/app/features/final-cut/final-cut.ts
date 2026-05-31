// Final Cut Smart Component
import { Component, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AnalyticsService } from '../../core/api/api/analytics.service';
import { WrappedSummaryDto } from '../../core/api/model/wrapped-summary-dto';

// Slides
import { IntroSlideComponent } from './slides/intro-slide/intro-slide';
import { GenreSlideComponent } from './slides/genre-slide/genre-slide';
import { DirectorSlideComponent } from './slides/director-slide/director-slide';
import { ActorSlideComponent } from './slides/actor-slide/actor-slide';
import { DecadeSlideComponent } from './slides/decade-slide/decade-slide';
import { SummarySlideComponent } from './slides/summary-slide/summary-slide';

@Component({
  selector: 'app-final-cut',
  standalone: true,
  imports: [
    CommonModule,
    IntroSlideComponent,
    GenreSlideComponent,
    DirectorSlideComponent,
    ActorSlideComponent,
    DecadeSlideComponent,
    SummarySlideComponent
  ],
  templateUrl: './final-cut.html',
  styleUrl: './final-cut.scss'
})
export class FinalCutComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private analytics = inject(AnalyticsService);

  public year = signal<number>(new Date().getFullYear() - 1);
  public data = signal<WrappedSummaryDto | null>(null);
  public loading = signal<boolean>(true);
  public activeSlide = signal<number>(0);

  public readonly SLIDE_COUNT = 6;
  public slideProgress = signal<number>(0);
  public isPaused = signal<boolean>(false);
  public isManuallyPaused = signal<boolean>(false);
  private progressInterval: any;
  private readonly SLIDE_DURATION_MS = 6000;
  private readonly TICK_MS = 50;

  constructor() {
    effect(() => {
      // Whenever year changes, fetch data
      const y = this.year();
      this.loadData(y);
    });
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const yearParam = params.get('year');
      if (yearParam) {
        this.year.set(parseInt(yearParam, 10));
      }
    });
  }

  ngOnDestroy() {
    this.stopTimer();
  }

  private startTimer() {
    this.stopTimer();
    this.slideProgress.set(0);
    this.progressInterval = setInterval(() => {
      if (this.isPaused() || this.isManuallyPaused() || this.loading()) return;
      
      const step = (this.TICK_MS / this.SLIDE_DURATION_MS) * 100;
      const current = this.slideProgress();
      if (current + step >= 100) {
        this.slideProgress.set(100);
        if (this.activeSlide() < this.SLIDE_COUNT - 1) {
          this.nextSlide();
        } else {
          this.stopTimer();
        }
      } else {
        this.slideProgress.set(current + step);
      }
    }, this.TICK_MS);
  }

  private stopTimer() {
    if (this.progressInterval) {
      clearInterval(this.progressInterval);
    }
  }

  private loadData(year: number) {
    this.loading.set(true);
    this.activeSlide.set(0);
    this.analytics.apiAnalyticsWrappedYearGet(year).subscribe({
      next: (res) => {
        this.data.set(res);
        this.loading.set(false);
        this.startTimer();
      },
      error: (err) => {
        console.error('Failed to load Final Cut data', err);
        this.loading.set(false);
      }
    });
  }

  public nextSlide() {
    if (this.activeSlide() < this.SLIDE_COUNT - 1) {
      this.activeSlide.update(v => v + 1);
      this.startTimer();
    }
  }

  public prevSlide() {
    if (this.activeSlide() > 0) {
      this.activeSlide.update(v => v - 1);
      this.startTimer();
    } else {
      this.slideProgress.set(0);
      this.startTimer();
    }
  }

  public togglePause() {
    this.isManuallyPaused.update(v => !v);
  }

  // Pointer event logic to distinguish between tap and hold
  private pointerDownTime = 0;

  public onPointerDown() {
    this.pointerDownTime = Date.now();
    this.isPaused.set(true);
  }

  public onPointerUp(direction: 'left' | 'right') {
    this.isPaused.set(false);
    const holdDuration = Date.now() - this.pointerDownTime;
    
    // If the click was short (less than 250ms), count it as a tap/click
    if (holdDuration < 250) {
      if (direction === 'right') {
        this.nextSlide();
      } else {
        this.prevSlide();
      }
    }
  }

  public onPointerLeave() {
    this.isPaused.set(false);
  }
}
