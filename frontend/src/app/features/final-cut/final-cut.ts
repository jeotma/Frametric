// Final Cut Smart Component — 20-slide cinematic experience
import { Component, OnInit, OnDestroy, inject, signal, effect, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AnalyticsService } from '../../core/api/api/analytics.service';
import { AdvancedAnalyticsService } from '../../core/api/api/advanced-analytics.service';
import { FinalCutService, FinalCutData } from '../../core/services/final-cut.service';
import { WrappedSummaryDto } from '../../core/api/model/wrapped-summary-dto';
import { AuthService } from '../../core/services/auth.service';
// Slides (original)
import { IntroSlideComponent } from './slides/intro-slide/intro-slide';

// Slides (new expanded)
import { BigNumbersSlideComponent } from './slides/big-numbers-slide/big-numbers-slide';
import { PrimeTimeSlideComponent } from './slides/prime-time-slide/prime-time-slide';
import { SlumpSlideComponent } from './slides/slump-slide/slump-slide';
import { WeekdayWarriorSlideComponent } from './slides/weekday-warrior-slide/weekday-warrior-slide';
import { GenreLandscapeSlideComponent } from './slides/genre-landscape-slide/genre-landscape-slide';
import { AListSlideComponent } from './slides/a-list-slide/a-list-slide';
import { AutoeursSlideComponent } from './slides/auteurs-slide/auteurs-slide';
import { DynamicDuosSlideComponent } from './slides/dynamic-duos-slide/dynamic-duos-slide';
import { BestRookiesSlideComponent } from './slides/best-rookies-slide/best-rookies-slide';
import { GenerationalDivideSlideComponent } from './slides/generational-divide-slide/generational-divide-slide';
import { ObsessionStreakSlideComponent } from './slides/obsession-streak-slide/obsession-streak-slide';
import { DavidGoliathSlideComponent } from './slides/david-goliath-slide/david-goliath-slide';
import { CinematicFatigueSlideComponent } from './slides/cinematic-fatigue-slide/cinematic-fatigue-slide';
import { BookendsSlideComponent } from './slides/bookends-slide/bookends-slide';
import { MonthlyExtremesSlideComponent } from './slides/monthly-extremes-slide/monthly-extremes-slide';
import { ReturnOfTheKingSlideComponent } from './slides/return-of-the-king-slide/return-of-the-king-slide';
import { HallOfFameSlideComponent } from './slides/hall-of-fame-slide/hall-of-fame-slide';
import { GoldenRaspberrySlideComponent } from './slides/golden-raspberry-slide/golden-raspberry-slide';
import { SummarySlideComponent } from './slides/summary-slide/summary-slide';

@Component({
  selector: 'app-final-cut',
  standalone: true,
  imports: [
    CommonModule,
    // Original slides
    IntroSlideComponent,
    // Slides Act I
    BigNumbersSlideComponent,
    PrimeTimeSlideComponent,
    SlumpSlideComponent,
    WeekdayWarriorSlideComponent,
    // Slides Act II
    GenreLandscapeSlideComponent,
    AListSlideComponent,
    AutoeursSlideComponent,
    DynamicDuosSlideComponent,
    BestRookiesSlideComponent,
    GenerationalDivideSlideComponent,
    // Slides Act III
    ObsessionStreakSlideComponent,
    DavidGoliathSlideComponent,
    CinematicFatigueSlideComponent,
    // Slides Act IV
    BookendsSlideComponent,
    MonthlyExtremesSlideComponent,
    ReturnOfTheKingSlideComponent,
    HallOfFameSlideComponent,
    GoldenRaspberrySlideComponent,
    SummarySlideComponent,
  ],
  templateUrl: './final-cut.html',
  styleUrl: './final-cut.scss'
})
export class FinalCutComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private analytics = inject(AnalyticsService);
  private finalCutService = inject(FinalCutService);
  private authService = inject(AuthService);

  public username = computed(() => this.authService.currentUser()?.username || 'User');

  public year = signal<number | 'global'>(new Date().getFullYear() - 1);
  public summary = signal<WrappedSummaryDto | null>(null);
  public extData = signal<FinalCutData | null>(null);
  public loading = signal<boolean>(true);
  public activeSlide = signal<number>(0);

  public readonly SLIDE_COUNT = 21;
  public slideProgress = signal<number>(0);
  public isPaused = signal<boolean>(false);
  public isManuallyPaused = signal<boolean>(false);
  private progressInterval: any;
  private readonly SLIDE_DURATION_MS = 8000;
  private readonly TICK_MS = 50;

  constructor() {
    effect(() => {
      const y = this.year();
      this.loadData(y);
    });
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const yearParam = params.get('year');
      if (yearParam) {
        if (yearParam === 'global') {
          this.year.set('global');
        } else {
          this.year.set(parseInt(yearParam, 10));
        }
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

  private loadData(year: number | 'global') {
    this.loading.set(true);
    this.activeSlide.set(0);

    forkJoin({
      summary: this.finalCutService.loadSummary(year),
      extended: this.finalCutService.loadAllData(year),
    }).subscribe({
      next: (res) => {
        this.summary.set(res.summary);
        this.extData.set(res.extended);
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

  private pointerDownTime = 0;

  public onPointerDown() {
    this.pointerDownTime = Date.now();
    this.isPaused.set(true);
  }

  public onPointerUp(direction: 'left' | 'right') {
    this.isPaused.set(false);
    const holdDuration = Date.now() - this.pointerDownTime;
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

  /** Slide index helper */
  public get slideIndices(): number[] {
    return Array.from({ length: this.SLIDE_COUNT }, (_, i) => i);
  }

  @HostListener('window:keydown.escape')
  handleEscape() {
    this.router.navigate(['/']);
  }
}
