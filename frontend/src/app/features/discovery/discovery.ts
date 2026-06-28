import { Component, inject, signal, OnInit, OnDestroy, computed, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, forkJoin, Observable } from 'rxjs';
import { finalize, delay, takeUntil } from 'rxjs/operators';
import { DiscoveryService } from '../../core/api/api/discovery.service';
import { SelectionResultDto } from '../../core/api/model/selection-result-dto';
import { MysteryBoxDto } from '../../core/api/model/mystery-box-dto';
import { BingoGridDto } from '../../core/api/model/bingo-grid-dto';
import { DiceRollResultDto } from '../../core/api/model/dice-roll-result-dto';
import { SlotMachineResultDto } from '../../core/api/model/slot-machine-result-dto';
import { AuthService } from '../../core/services/auth.service';
import { ModalService } from '../../core/services/modal.service';
import { CinematicSelectComponent } from '../../components/cinematic-select/cinematic-select.component';
import { CinematicToggleComponent } from '../../components/cinematic-toggle/cinematic-toggle.component';
import { CinematicTooltipComponent } from '../../components/cinematic-tooltip/cinematic-tooltip.component';
import { CustomSelectionInputComponent } from './components/custom-selection-input/custom-selection-input.component';
import { CustomListToastComponent, ToastCustomListWinner } from './components/custom-list-toast/custom-list-toast.component';
import { RouletteWheelComponent } from './components/roulette-wheel/roulette-wheel.component';
import { DiceRollerComponent } from './components/dice-roller/dice-roller.component';
import { SlotReelsComponent } from './components/slot-reels/slot-reels.component';
import { MysteryGridComponent } from './components/mystery-grid/mystery-grid.component';
import { MovieSimpleDto } from '../../core/api/model/movie-simple-dto';
import { CustomListsService } from '../../core/api/api/custom-lists.service';
import { CustomListDto } from '../../core/api/model/custom-list-dto';
import { RouletteRaceResultDto } from '../../core/api/model/roulette-race-result-dto';
import { BingoBoardDto } from '../../core/api/model/bingo-board-dto';
import { DiscoveryAudioService } from './services/discovery-audio.service';

type DiscoveryTab = 'roulette' | 'dice' | 'slot-machine' | 'mystery-box' | 'bingo';

@Component({
  selector: 'app-discovery',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    RouterLink,
    CinematicSelectComponent,
    CinematicToggleComponent,
    CinematicTooltipComponent,
    CustomSelectionInputComponent,
    CustomListToastComponent,
    RouletteWheelComponent,
    DiceRollerComponent,
    SlotReelsComponent,
    MysteryGridComponent
  ],
  templateUrl: './discovery.html',
  styleUrl: './discovery.scss'
})
export class DiscoveryComponent implements OnInit, OnDestroy {
  private discoveryService = inject(DiscoveryService);
  private customListsService = inject(CustomListsService);
  public auth = inject(AuthService);
  public modalService = inject(ModalService);
  public audioService = inject(DiscoveryAudioService);
  private destroy$ = new Subject<void>();

  public activeTab = signal<DiscoveryTab>('roulette');
  public userCustomLists: CustomListDto[] = [];
  public activeToasts = signal<ToastCustomListWinner[]>([]);
  public removingToasts = signal<Record<string, boolean>>({});
  public errorMsg = signal<string | null>(null);

  private tabMap: Record<string, DiscoveryTab> = {
    bingo: 'bingo', roulette: 'roulette', 'mystery-box': 'mystery-box',
    dice: 'dice', 'slot-machine': 'slot-machine',
  };

  public scopeOptions = [
    { value: 0, label: 'Personal Watchlist' },
    { value: 1, label: 'Global Database' },
    { value: 2, label: 'Custom Selection' },
    { value: 4, label: 'Merge Watchlists' }
  ];
  public diceScopeOptions = [
    { value: 0, label: 'Personal Watchlist' },
    { value: 1, label: 'Global Database' },
    { value: 2, label: 'Custom Selection' },
    { value: 4, label: 'Merge Watchlists' }
  ];
  public bingoGridOptions = [
    { value: 3, label: '3x3 Grid' },
    { value: 4, label: '4x4 Grid' },
    { value: 5, label: '5x5 Grid' }
  ];
  public mysteryVariantOptions = [
    { value: 0, label: 'Classic Box' },
    { value: 1, label: 'Genre Theme' },
    { value: 2, label: 'Actor Focus' },
    { value: 3, label: 'Director Focus' }
  ];

  // Roulette state
  public rouletteWinnerSig = signal<SelectionResultDto | null>(null);
  public rouletteWinnersSig = signal<SelectionResultDto[]>([]);
  public rouletteAllowMultipleWinners = signal<boolean>(false);
  public rouletteWinnerCount = signal<number>(1);
  public rouletteIsRacing = signal(false);
  public rouletteLoading = signal(false);
  public rouletteScope = signal<number>(0);
  public rouletteThreshold = signal<number>(1);
  public rouletteExcludeWatched = signal<boolean>(true);
  public rouletteChips = signal<MovieSimpleDto[]>([]);
  public roulettePartnerUsername = signal<string>('');
  public rouletteSequenceSig = signal<MovieSimpleDto[]>([]);
  public rouletteWinnerTitleSig = signal<string | null>(null);
  public rawRouletteWinner = signal<SelectionResultDto | null>(null);
  public rawRouletteWinners = signal<SelectionResultDto[]>([]);
  public rouletteLeaderboard = signal<{ rank: number; title: string; count: number }[]>([]);
  public rouletteRaceIndex = 0;
  public rouletteNicknameMap: Map<string, string> = new Map();
  public rouletteIsFullscreen = signal<boolean>(false);
  private roulettePanelEl: HTMLElement | null = null;

  // Dice state
  public diceResultSig = signal<DiceRollResultDto | null>(null);
  public diceLoading = signal(false);
  public diceIsRolling = signal(false);
  public diceScope = signal<number>(0);
  public diceExcludeWatched = signal<boolean>(true);
  public diceChips = signal<MovieSimpleDto[]>([]);
  public dicePartnerUsername = signal<string>('');
  public diceValues = signal<number[]>([0, 0, 0, 0, 0]);
  public diceLabels = signal<string[]>(['', '', '', '', '']);
  public diceSettled = signal<boolean[]>([false, false, false, false, false]);
  public diceRolling = signal<boolean[]>([false, false, false, false, false]);
  public diceMuted = signal(false);
  
  public pendingCriticalChoice = signal<boolean>(false);
  public diceSpecialStatusMsg = signal<string | null>(null);
  public hasAutomaticFumbleRerolled = false;

  // Slot Machine state
  public slotResultSig = signal<SlotMachineResultDto | null>(null);
  public slotLoading = signal(false);
  public slotIsSpinning = signal(false);
  public slotScope = signal<number>(0);
  
  public slotGenre = signal<string>('');
  public slotDecadeVal = signal<number | null>(null);
  public slotPopularity = signal<string>('');
  public slotRating = signal<string>('');
  public slotCountry = signal<string>('');

  public slotGenreLocked = signal<boolean>(false);
  public slotDecadeLocked = signal<boolean>(false);
  public slotPopularityLocked = signal<boolean>(false);
  public slotRatingLocked = signal<boolean>(false);
  public slotCountryLocked = signal<boolean>(false);

  public slotExcludeWatched = signal<boolean>(true);
  public slotChips = signal<MovieSimpleDto[]>([]);

  public slotGenreOptions = [
    { value: '', label: 'Any Genre' },
    { value: 'Action', label: 'Action' },
    { value: 'Adventure', label: 'Adventure' },
    { value: 'Animation', label: 'Animation' },
    { value: 'Comedy', label: 'Comedy' },
    { value: 'Crime', label: 'Crime' },
    { value: 'Documentary', label: 'Documentary' },
    { value: 'Drama', label: 'Drama' },
    { value: 'Family', label: 'Family' },
    { value: 'Fantasy', label: 'Fantasy' },
    { value: 'History', label: 'History' },
    { value: 'Horror', label: 'Horror' },
    { value: 'Music', label: 'Music' },
    { value: 'Mystery', label: 'Mystery' },
    { value: 'Romance', label: 'Romance' },
    { value: 'Science Fiction', label: 'Science Fiction' },
    { value: 'TV Movie', label: 'TV Movie' },
    { value: 'Thriller', label: 'Thriller' },
    { value: 'War', label: 'War' },
    { value: 'Western', label: 'Western' }
  ];

  public slotDecadeOptions = [
    { value: null, label: 'Any Decade' },
    { value: 1980, label: '1980s' },
    { value: 1990, label: '1990s' },
    { value: 2000, label: '2000s' },
    { value: 2010, label: '2010s' },
    { value: 2020, label: '2020s' }
  ];
  
  public slotPopularityOptions = [
    { value: '', label: 'Any Popularity' },
    { value: 'BLOCKBUSTER', label: 'Blockbuster' },
    { value: 'MAINSTREAM', label: 'Mainstream' },
    { value: 'NICHE / CULT', label: 'Niche / Cult' },
    { value: 'HIDDEN GEM', label: 'Hidden Gem' }
  ];

  public slotRatingOptions = [
    { value: '', label: 'Any Rating' },
    { value: 'MASTERPIECE', label: 'Masterpiece' },
    { value: 'GREAT', label: 'Great' },
    { value: 'DECENT', label: 'Decent' },
    { value: 'UNDERDOG', label: 'Underdog' }
  ];

  // Mystery Box state
  public mysteryResultSig = signal<MysteryBoxDto | null>(null);
  public revealedMovieSig = signal<SelectionResultDto | null>(null);
  public mysteryLoading = signal(false);
  public mysteryRevealing = signal(false);
  public mysteryScope = signal<number>(0);
  public mysteryVariant = signal<number>(0);
  public selectedBoxId = signal<string | null>(null);
  public mysteryExcludeWatched = signal<boolean>(true);
  public mysteryChips = signal<MovieSimpleDto[]>([]);
  public mysteryPartnerUsername = signal<string>('');
  public revealedOthersMap = signal<Record<string, SelectionResultDto>>({});
  public isRevealingOthers = signal<boolean>(false);
  public hasRevealedOthers = computed(() => Object.keys(this.revealedOthersMap()).length > 0);

  // Custom Confirmation Modal state
  public showConfirmModal = signal<boolean>(false);
  public confirmTitle = signal<string>('Confirmation');
  public confirmMessage = signal<string>('');
  public confirmOptions = signal<{ label: string; value: string; type: string }[]>([]);
  private confirmResolveFn: ((value: string) => void) | null = null;

  public showConfirm(title: string, message: string, options: { label: string; value: string; type: string }[]): Promise<string> {
    this.confirmTitle.set(title);
    this.confirmMessage.set(message);
    this.confirmOptions.set(options);
    this.showConfirmModal.set(true);
    return new Promise<string>((resolve) => {
      this.confirmResolveFn = resolve;
    });
  }

  public resolveConfirm(value: string): void {
    this.showConfirmModal.set(false);
    if (this.confirmResolveFn) {
      this.confirmResolveFn(value);
      this.confirmResolveFn = null;
    }
  }

  public getSavedBoardIds(): string[] {
    try {
      const saved = localStorage.getItem('frametric_saved_bingo_boards');
      return saved ? JSON.parse(saved) : [];
    } catch {
      return [];
    }
  }

  public saveBoardId(boardId: string): void {
    try {
      const saved = this.getSavedBoardIds();
      if (!saved.includes(boardId)) {
        saved.push(boardId);
        localStorage.setItem('frametric_saved_bingo_boards', JSON.stringify(saved));
      }
    } catch (e) {
      console.warn('Failed to save board ID', e);
    }
  }

  // Bingo state
  public bingoResultSig = signal<BingoGridDto | null>(null);
  public bingoLoading = signal(false);
  public bingoGridSize = signal<number>(3);
  public bingoScope = signal<number>(0);
  public bingoExcludeWatched = signal<boolean>(true);
  public bingoChips = signal<MovieSimpleDto[]>([]);
  public bingoDurationDays = signal<number | null>(null);
  
  public bingoBoards = signal<BingoBoardDto[]>([]);
  public activeBingoBoards = computed(() => {
    const now = new Date();
    return this.bingoBoards().filter(b => 
      !b.isCompleted && (!b.endDate || new Date(b.endDate) >= now)
    );
  });
  public completedBingoBoards = computed(() => {
    return this.bingoBoards().filter(b => 
      b.isCompleted && (b.completedOnTime ?? true)
    );
  });
  public expiredFailedBingoBoards = computed(() => {
    const now = new Date();
    return this.bingoBoards().filter(b => 
      (!b.isCompleted && b.endDate && new Date(b.endDate) < now) ||
      (b.isCompleted && b.completedOnTime === false)
    );
  });

  // Candidate Selection Modal state
  public showCandidatesModal = signal<boolean>(false);
  public candidatesLoading = signal<boolean>(false);
  public candidateSquares = signal<any[]>([]);
  public selectedObjectiveId: string | null = null;
  public selectedSquare: any | null = null;
  
  public bingoDurationOptions = [
    { value: null, label: 'Unlimited' },
    { value: 7, label: '7 Days' },
    { value: 14, label: '14 Days' },
    { value: 30, label: '30 Days' }
  ];

  public bingoCountdownMessage = computed(() => {
    const res = this.bingoResultSig();
    if (!res || !res.endDate) return null;
    const end = new Date(res.endDate);
    const now = new Date();
    const diffTime = end.getTime() - now.getTime();
    if (diffTime <= 0) {
      return 'EXPIRED';
    }
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return `${diffDays} day${diffDays === 1 ? '' : 's'} remaining`;
  });

  public maxBingoRerolls = computed(() => {
    const size = this.bingoResultSig()?.gridSize ?? this.bingoGridSize();
    return size === 3 ? 1 : size === 4 ? 2 : 3;
  });

  public diceMatchStatus = computed(() => {
    const res = this.diceResultSig();
    if (!res || res.matchDistance === undefined || !this.diceSettled().every(s => s)) return null;
    
    const dist = res.matchDistance;
    if (dist === 0) {
      return {
        text: '[SYSTEM: PERFECT MATCH] Exact alignment with rolled coordinates.',
        class: 'match-perfect',
        color: 'var(--accent-emerald)'
      };
    }
    if (dist >= 1 && dist <= 2) {
      return {
        text: '[SYSTEM: APPROXIMATE MATCH] Near alignment. Adjusted constraints slightly.',
        class: 'match-near',
        color: 'var(--accent-sepia)'
      };
    }
    if (dist >= 3 && dist <= 4) {
      return {
        text: '[SYSTEM: DEVIATED MATCH] Broad alignment. Multiple constraints relaxed.',
        class: 'match-deviated',
        color: 'rgba(229, 9, 20, 0.6)'
      };
    }
    if (dist >= 5 && dist <= 6) {
      return {
        text: '[SYSTEM: EXTENDED MATCH] Wide alignment. Core constraints relaxed.',
        class: 'match-extended',
        color: 'rgba(229, 9, 20, 0.75)'
      };
    }
    if (dist >= 7 && dist <= 12) {
      return {
        text: '[SYSTEM: MAXIMUM DEVIATION] Extreme alignment deviation. Searching pool with minimum constraints.',
        class: 'match-maximum',
        color: 'rgba(229, 9, 20, 0.9)'
      };
    }
    return {
      text: '[SYSTEM: CALIBRATION FAULT] No matching criteria found. Full database selection.',
      class: 'match-fault',
      color: 'var(--accent-record)'
    };
  });

  public winnerModalMovies = signal<any[]>([]);
  public winnerModalMovie = computed(() => {
    const movies = this.winnerModalMovies();
    return movies.length === 1 ? movies[0] : null;
  });

  public closeWinnerModal(): void {
    this.winnerModalMovies.set([]);
  }

  public slotCountryOptions = signal<{ value: string; label: string }[]>([
    { value: '', label: 'Any Country' }
  ]);

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.loadUserLists();
      this.loadAvailableCountries();
      this.loadBingoBoards();
    }
  }

  private loadAvailableCountries(): void {
    this.discoveryService.apiV1DiscoveryAvailableCountriesGet().pipe(takeUntil(this.destroy$)).subscribe({
      next: (countries) => {
        const opts = [
          { value: '', label: 'Any Country' },
          ...countries.map(c => ({ value: c, label: c }))
        ];
        this.slotCountryOptions.set(opts);
      },
      error: () => {
        console.warn('Failed to load available countries for slot machine.');
      }
    });
  }

  private loadUserLists(): void {
    this.customListsService.apiV1CustomListsGet().pipe(takeUntil(this.destroy$)).subscribe({
      next: (lists) => {
        this.userCustomLists = lists;
      },
      error: () => {
        console.warn('Failed to load user custom lists for discovery interception.');
      }
    });
  }

  private checkWinnerAgainstLists(winner: SelectionResultDto): void {
    if (!winner || !winner.movieId || !Array.isArray(this.userCustomLists)) return;
    const movieId = winner.movieId;
    const newToasts: ToastCustomListWinner[] = [];

    for (const list of this.userCustomLists) {
      if (list.movies?.some((m: MovieSimpleDto) => m.id === movieId)) {
        newToasts.push({
          movieId: movieId,
          movieTitle: winner.title || 'Unknown',
          listId: list.id || '',
          listName: list.name || 'Unknown List'
        });
      }
    }

    if (newToasts.length > 0) {
      this.activeToasts.update(current => {
        const result = [...current];
        for (const nt of newToasts) {
          if (!result.some(t => t.listId === nt.listId && t.movieId === nt.movieId)) {
            result.push(nt);
          }
        }
        return result;
      });
    }
  }

  public removeWinnerFromList(toast: ToastCustomListWinner): void {
    const key = `${toast.listId}-${toast.movieId}`;
    this.removingToasts.update(r => ({ ...r, [key]: true }));

    this.customListsService.apiV1CustomListsIdMoviesMovieIdDelete(
      toast.listId,
      toast.movieId
    ).pipe(
      takeUntil(this.destroy$),
      finalize(() => {
        this.removingToasts.update(r => {
          const nr = { ...r };
          delete nr[key];
          return nr;
        });
      })
    ).subscribe({
      next: () => {
        this.dismissToast(toast);
        const list = this.userCustomLists.find(l => l.id === toast.listId);
        if (list && list.movies) {
          list.movies = list.movies.filter((m: MovieSimpleDto) => m.id !== toast.movieId);
        }
      },
      error: () => {
        this.errorMsg.set(`Failed to remove movie from list: ${toast.listName}`);
      }
    });
  }

  public dismissToast(toast: ToastCustomListWinner): void {
    this.activeToasts.update(current => 
      current.filter(t => !(t.listId === toast.listId && t.movieId === toast.movieId))
    );
  }

  public setActiveTab(tab: string): void {
    const mapped = this.tabMap[tab];
    if (mapped) this.activeTab.set(mapped);
  }

  private getChipsTitles(chips: MovieSimpleDto[]): string[] | undefined {
    if (chips.length === 0) return undefined;
    return chips.map(c => c.title!).filter(t => !!t);
  }

  private getChipsIds(chips: MovieSimpleDto[]): string[] | undefined {
    if (chips.length === 0) return undefined;
    return chips.map(c => c.id!).filter(id => !!id);
  }

  public readonly MAX_ROULETTE_STEPS = 50;
  public rouletteStepCount = signal(0);
  public rouletteSkipped = signal(false);

  public spellRoulette(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.rouletteLoading.set(true);
    this.rouletteSkipped.set(false);

    // Build custom aliases dict (Guid -> alias) for custom scope
    let customAliases: { [key: string]: string } | undefined = undefined;
    if (this.rouletteScope() === 2 && this.rouletteNicknameMap.size > 0) {
      customAliases = {};
      this.rouletteNicknameMap.forEach((alias, id) => {
        customAliases![id] = alias;
      });
    }

    this.discoveryService.apiV1DiscoveryRoulettePost({
      scope: this.rouletteScope() as any,
      winningThreshold: this.rouletteThreshold() > 1 ? this.rouletteThreshold() : 1,
      excludeWatched: this.rouletteExcludeWatched(),
      customSourceIds: this.rouletteScope() === 2 ? this.getChipsIds(this.rouletteChips()) : undefined,
      customSourceTitles: this.rouletteScope() === 2 ? this.getChipsTitles(this.rouletteChips()) : undefined,
      customAliases: customAliases,
      partnerUsername: this.rouletteScope() === 4 ? this.roulettePartnerUsername() || null : undefined,
      allowMultipleWinners: this.rouletteAllowMultipleWinners(),
      winnerCount: this.rouletteWinnerCount()
    }).pipe(takeUntil(this.destroy$)).subscribe({ 
      next: (r: RouletteRaceResultDto) => {
        this.rouletteLoading.set(false);
        if (!r.spinSequence || r.spinSequence.length === 0) return;
        
        this.rouletteWinnerSig.set(null);
        this.rouletteWinnersSig.set([]);
        
        // Truncate sequence to prevent infinite spinning — keep first N + winner
        const winners = r.winners || (r.winner ? [r.winner] : []);
        const winner = winners[0] || r.winner || r.spinSequence[r.spinSequence.length - 1];
        let seq = r.spinSequence;
        if (this.rouletteThreshold() > 1 && seq.length > this.MAX_ROULETTE_STEPS) {
          const poolSlices = seq.filter(s => s.selectionMechanismMetadata === 'Initial candidate');
          const raceSlices = seq.slice(poolSlices.length, this.MAX_ROULETTE_STEPS - poolSlices.length);
          seq = [...poolSlices, ...raceSlices, winner];
        }
        this.rouletteSequenceSig.set(seq);
        
        this.rawRouletteWinner.set(winner);
        this.rawRouletteWinners.set(winners);
        
        this.rouletteRaceIndex = 0;
        this.rouletteStepCount.set(0);
        this.rouletteLeaderboard.set([]);
        this.audioService.playLeverPull();
        this.runNextRouletteRaceStep();
      }, 
      error: e => {
        this.rouletteLoading.set(false);
        this.errorMsg.set(e.error?.error || 'Roulette failed');
      } 
    });
  }

  public skipRouletteRace(): void {
    this.rouletteSkipped.set(true);
    const winner = this.rawRouletteWinner();
    this.rouletteRaceIndex = this.rouletteSequenceSig().length - 1;
    this.rouletteStepCount.set(this.rouletteRaceIndex);
    this.rouletteWinnerTitleSig.set(winner?.title || '');
    this.rouletteIsRacing.set(true);
  }

  private runNextRouletteRaceStep(): void {
    const seq = this.rouletteSequenceSig();
    const isThresholdRace = this.rouletteThreshold() > 1;

    if (!isThresholdRace) {
      // Spin directly to the final winner!
      const winner = this.rawRouletteWinner();
      this.rouletteWinnerTitleSig.set(winner?.title || '');
      this.rouletteIsRacing.set(true);
      this.rouletteRaceIndex = seq.length - 1; // Mark as done
      return;
    }

    // Threshold race: spin step-by-step
    if (this.rouletteRaceIndex >= seq.length) return;

    const currentRoll = seq[this.rouletteRaceIndex];
    this.rouletteWinnerTitleSig.set(currentRoll.title || '');
    this.rouletteStepCount.set(this.rouletteRaceIndex + 1);

    // Trigger rouletteIsRacing to true for the first step, or trigger a re-spin
    if (this.rouletteRaceIndex === 0) {
      this.rouletteIsRacing.set(true);
    }
  }

  public onRouletteFinished(): void {
    if (this.rouletteSkipped()) {
      this.rouletteIsRacing.set(false);
      const winners = this.rawRouletteWinners();
      if (winners.length > 0) {
        this.rouletteWinnersSig.set(winners);
        this.rouletteWinnerSig.set(winners[0]);
        winners.forEach(w => this.checkWinnerAgainstLists(w));
        this.winnerModalMovies.set(winners);
        this.audioService.playSuccess();
      }
      return;
    }

    const seq = this.rouletteSequenceSig();
    const isThresholdRace = this.rouletteThreshold() > 1;

    if (isThresholdRace) {
      // Update leaderboard counts up to this completed roll
      const counts: Record<string, number> = {};
      for (let i = 0; i <= this.rouletteRaceIndex; i++) {
        const title = seq[i].title || 'Unknown';
        counts[title] = (counts[title] || 0) + 1;
      }

      // Convert to sorted leaderboard list
      const list = Object.entries(counts)
        .map(([title, count]) => ({ title, count }))
        .sort((a, b) => b.count - a.count || a.title.localeCompare(b.title));

      const rankedList = list.map((item, idx) => ({
        rank: idx + 1,
        title: item.title,
        count: item.count
      }));

      this.rouletteLeaderboard.set(rankedList);
    }

    if (isThresholdRace && this.rouletteRaceIndex < seq.length - 1) {
      // Play a tik/clack sound on each intermediate spin
      this.audioService.playTick(1.0 + (this.rouletteRaceIndex % 5) * 0.05);

      // Staggered delay for the user to read leaderboard and then do next spin
      setTimeout(() => {
        this.rouletteRaceIndex++;
        this.runNextRouletteRaceStep();
      }, 1800);
    } else {
      // Race finished! Show the final winner
      this.rouletteIsRacing.set(false);
      const winners = this.rawRouletteWinners();
      if (winners.length > 0) {
        this.rouletteWinnersSig.set(winners);
        this.rouletteWinnerSig.set(winners[0]);
        winners.forEach(w => this.checkWinnerAgainstLists(w));
        this.winnerModalMovies.set(winners);
        this.audioService.playSuccess();
      }
    }
  }

  public rollDice(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.resetDiceState();
    this.diceLoading.set(true);
    
    // Set all dice to rolling
    this.diceRolling.set([true, true, true, true, true]);
    this.diceSettled.set([false, false, false, false, false]);
    this.diceValues.set([0, 0, 0, 0, 0]);
    this.diceLabels.set(['', '', '', '', '']);

    this.discoveryService.apiV1DiscoveryDicePost({ 
      scope: this.diceScope() as any,
      excludeWatched: this.diceExcludeWatched(),
      customSourceIds: this.diceScope() === 2 ? this.getChipsIds(this.diceChips()) : undefined,
      customSourceTitles: this.diceScope() === 2 ? this.getChipsTitles(this.diceChips()) : undefined,
      partnerUsername: this.diceScope() === 4 ? this.dicePartnerUsername() || null : undefined
    }).pipe(takeUntil(this.destroy$)).subscribe({ 
        next: r => {
          this.diceResultSig.set(r);
          // Staggered stop for all dice
          let delayAcc = 1000;
          r.diceResults.forEach((d) => {
            const idx = d.diceType;
            setTimeout(() => {
              this.diceRolling.update(arr => { const na = [...arr]; na[idx] = false; return na; });
              this.diceValues.update(arr => { const na = [...arr]; na[idx] = d.rollValue; return na; });
              this.diceLabels.update(arr => { const na = [...arr]; na[idx] = d.label; return na; });
              this.diceSettled.update(arr => { const na = [...arr]; na[idx] = true; return na; });
            }, delayAcc);
            delayAcc += 350;
          });

          setTimeout(() => {
            this.onDiceFinished();
          }, delayAcc + 200);
        }, 
        error: e => {
          this.diceLoading.set(false);
          this.diceRolling.set([false, false, false, false, false]);
          this.errorMsg.set(e.error?.error || 'Dice roll failed');
        }
      });
  }

  public onDieClicked(idx: number): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    if (this.diceLoading()) return;

    // Handle Critical Roll manual reroll selection
    if (this.pendingCriticalChoice()) {
      const maxVals = [3, 4, 6, 12, 20];
      if (this.diceValues()[idx] === maxVals[idx]) {
        this.errorMsg.set("Cannot reroll a Critical Roll die!");
        return;
      }
      this.pendingCriticalChoice.set(false);
      this.diceSpecialStatusMsg.set(null);
      this.triggerSingleReroll(idx);
      return;
    }
    
    // If all dice are settled, reset the state to start a new roll sequence
    if (this.diceSettled().every(s => s)) {
      this.resetDiceState();
    }
    
    if (this.diceRolling()[idx] || this.diceSettled()[idx]) return;

    this.errorMsg.set(null);

    // If we don't have results yet, fetch them from the API!
    if (!this.diceResultSig()) {
      this.diceLoading.set(true);
      // Mark this die as rolling visually while loading
      this.diceRolling.update(arr => { const na = [...arr]; na[idx] = true; return na; });

      this.discoveryService.apiV1DiscoveryDicePost({ 
        scope: this.diceScope() as any,
        excludeWatched: this.diceExcludeWatched(),
        customSourceIds: this.diceScope() === 2 ? this.getChipsIds(this.diceChips()) : undefined,
        customSourceTitles: this.diceScope() === 2 ? this.getChipsTitles(this.diceChips()) : undefined,
        partnerUsername: this.diceScope() === 4 ? this.dicePartnerUsername() || null : undefined
      }).pipe(takeUntil(this.destroy$)).subscribe({ 
        next: r => {
          this.diceResultSig.set(r);
          this.diceLoading.set(false);

          // Get the outcome for the clicked die
          const d = r.diceResults.find(res => res.diceType === idx);
          if (d) {
            // Let it roll for at least 1s total to feel organic
            setTimeout(() => {
              this.diceRolling.update(arr => { const na = [...arr]; na[idx] = false; return na; });
              this.diceValues.update(arr => { const na = [...arr]; na[idx] = d.rollValue; return na; });
              this.diceLabels.update(arr => { const na = [...arr]; na[idx] = d.label; return na; });
              this.diceSettled.update(arr => { const na = [...arr]; na[idx] = true; return na; });
              
              this.checkIfAllDiceSettled();
            }, 1000);
          }
        }, 
        error: e => {
          this.diceLoading.set(false);
          this.diceRolling.update(arr => { const na = [...arr]; na[idx] = false; return na; });
          this.errorMsg.set(e.error?.error || 'Dice roll failed');
        }
      });
    } else {
      // We already have the result, roll and reveal just this one!
      this.diceRolling.update(arr => { const na = [...arr]; na[idx] = true; return na; });
      const d = this.diceResultSig()?.diceResults.find(res => res.diceType === idx);
      if (d) {
        setTimeout(() => {
          this.diceRolling.update(arr => { const na = [...arr]; na[idx] = false; return na; });
          this.diceValues.update(arr => { const na = [...arr]; na[idx] = d.rollValue; return na; });
          this.diceLabels.update(arr => { const na = [...arr]; na[idx] = d.label; return na; });
          this.diceSettled.update(arr => { const na = [...arr]; na[idx] = true; return na; });

          this.checkIfAllDiceSettled();
        }, 1000);
      }
    }
  }

  private triggerSingleReroll(idx: number): void {
    const currentResults = this.diceResultSig()?.diceResults;
    if (!currentResults) return;

    this.diceLoading.set(true);
    this.diceRolling.update(arr => { const na = [...arr]; na[idx] = true; return na; });
    this.diceSettled.update(arr => { const na = [...arr]; na[idx] = false; return na; });

    // Build presets dictionary mapping all OTHER dice to their current values
    const presets: Record<string, number> = {};
    currentResults.forEach(d => {
      if (d.diceType !== idx) {
        presets[d.diceType.toString()] = d.rollValue;
      }
    });

    this.discoveryService.apiV1DiscoveryDicePost({
      scope: this.diceScope() as any,
      excludeWatched: this.diceExcludeWatched(),
      customSourceIds: this.diceScope() === 2 ? this.getChipsIds(this.diceChips()) : undefined,
      customSourceTitles: this.diceScope() === 2 ? this.getChipsTitles(this.diceChips()) : undefined,
      presets: presets,
      partnerUsername: this.diceScope() === 4 ? this.dicePartnerUsername() || null : undefined
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: r => {
        this.diceResultSig.set(r);
        this.diceLoading.set(false);

        const d = r.diceResults.find(res => res.diceType === idx);
        if (d) {
          setTimeout(() => {
            this.diceRolling.update(arr => { const na = [...arr]; na[idx] = false; return na; });
            this.diceValues.update(arr => { const na = [...arr]; na[idx] = d.rollValue; return na; });
            this.diceLabels.update(arr => { const na = [...arr]; na[idx] = d.label; return na; });
            this.diceSettled.update(arr => { const na = [...arr]; na[idx] = true; return na; });

            this.onDiceFinished();
          }, 1000);
        }
      },
      error: e => {
        this.diceLoading.set(false);
        this.diceRolling.update(arr => { const na = [...arr]; na[idx] = false; return na; });
        this.errorMsg.set(e.error?.error || 'Reroll failed');
      }
    });
  }

  private checkIfAllDiceSettled(): void {
    if (this.diceSettled().every(s => s)) {
      this.onDiceFinished();
    }
  }

  public resetDiceState(): void {
    this.diceResultSig.set(null);
    this.diceValues.set([0, 0, 0, 0, 0]);
    this.diceLabels.set(['', '', '', '', '']);
    this.diceSettled.set([false, false, false, false, false]);
    this.diceRolling.set([false, false, false, false, false]);
    this.pendingCriticalChoice.set(false);
    this.diceSpecialStatusMsg.set(null);
    this.hasAutomaticFumbleRerolled = false;
  }

  public onDiceFinished(): void {
    this.diceLoading.set(false);
    this.diceIsRolling.set(false);
    const r = this.diceResultSig();
    if (!r) return;

    // Check for critical rolls and fumbles
    const maxVals = [3, 4, 6, 12, 20];
    const criticalIndices: number[] = [];
    const fumbleIndices: number[] = [];

    r.diceResults.forEach(d => {
      const idx = d.diceType;
      const val = d.rollValue;
      if (val === maxVals[idx]) {
        criticalIndices.push(idx);
      } else if (val === 1) {
        fumbleIndices.push(idx);
      }
    });

    // Handle Fumble first (automatic reroll of the highest value die that is not a fumble)
    if (fumbleIndices.length > 0 && !this.hasAutomaticFumbleRerolled) {
      // Find the highest non-fumble die
      let highestIdx = -1;
      let highestVal = -1;
      r.diceResults.forEach(d => {
        const idx = d.diceType;
        if (!fumbleIndices.includes(idx) && d.rollValue > highestVal) {
          highestVal = d.rollValue;
          highestIdx = idx;
        }
      });

      if (highestIdx !== -1) {
        this.hasAutomaticFumbleRerolled = true;
        const diceNames = ['Duration (D3)', 'Popularity (D4)', 'Risk (D6)', 'Quality (D12)', 'Genre (D20)'];
        const fumbleName = fumbleIndices.map(fi => diceNames[fi]).join(', ');
        this.diceSpecialStatusMsg.set(`FUMBLE on ${fumbleName}! Rerolling highest die: ${diceNames[highestIdx]}...`);

        setTimeout(() => {
          this.triggerSingleReroll(highestIdx);
        }, 1500);
        return; // Pause final winner modal
      }
    }

    // Handle Critical Rolls (user picks a die to reroll)
    if (criticalIndices.length > 0 && !this.pendingCriticalChoice()) {
      const otherDiceIndices = [0, 1, 2, 3, 4].filter(idx => !criticalIndices.includes(idx));
      if (otherDiceIndices.length > 0) {
        const diceNames = ['Duration (D3)', 'Popularity (D4)', 'Risk (D6)', 'Quality (D12)', 'Genre (D20)'];
        const critName = criticalIndices.map(ci => diceNames[ci]).join(', ');
        this.diceSpecialStatusMsg.set(`CRITICAL ROLL on ${critName}! Click any other die to reroll it.`);
        this.pendingCriticalChoice.set(true);
        return; // Pause final winner modal
      }
    }

    // No fumbles/criticals or they are fully resolved, reveal the movie!
    this.diceSpecialStatusMsg.set(null);
    this.diceRolling.set([false, false, false, false, false]);
    this.audioService.playSuccess();
    this.checkWinnerAgainstLists(r);
    this.winnerModalMovies.set([r]);
  }

  public get totalLockedSlots(): number {
    return (this.slotGenreLocked() ? 1 : 0) +
           (this.slotDecadeLocked() ? 1 : 0) +
           (this.slotPopularityLocked() ? 1 : 0) +
           (this.slotRatingLocked() ? 1 : 0) +
           (this.slotCountryLocked() ? 1 : 0);
  }

  public toggleSlotLock(reel: 'genre' | 'decade' | 'popularity' | 'rating' | 'country'): void {
    if (reel === 'genre') {
      if (!this.slotGenreLocked() && this.totalLockedSlots >= 4) return;
      this.slotGenreLocked.update(v => !v);
    } else if (reel === 'decade') {
      if (!this.slotDecadeLocked() && this.totalLockedSlots >= 4) return;
      this.slotDecadeLocked.update(v => !v);
    } else if (reel === 'popularity') {
      if (!this.slotPopularityLocked() && this.totalLockedSlots >= 4) return;
      this.slotPopularityLocked.update(v => !v);
    } else if (reel === 'rating') {
      if (!this.slotRatingLocked() && this.totalLockedSlots >= 4) return;
      this.slotRatingLocked.update(v => !v);
    } else if (reel === 'country') {
      if (!this.slotCountryLocked() && this.totalLockedSlots >= 4) return;
      this.slotCountryLocked.update(v => !v);
    }
  }

  public spinSlots(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.slotLoading.set(true);
    this.slotIsSpinning.set(true);
    this.slotResultSig.set(null);
    this.discoveryService.apiV1DiscoverySlotMachinePost({
      scope: this.slotScope() as any,
      genre: this.slotGenreLocked() && this.slotGenre() ? this.slotGenre() : null,
      decade: this.slotDecadeLocked() && this.slotDecadeVal() !== null ? this.slotDecadeVal() : null,
      popularity: this.slotPopularityLocked() && this.slotPopularity() ? this.slotPopularity() as any : null,
      rating: this.slotRatingLocked() && this.slotRating() ? this.slotRating() as any : null,
      country: this.slotCountryLocked() && this.slotCountry() ? this.slotCountry() : null,
      excludeWatched: this.slotExcludeWatched(),
      customSourceIds: this.slotScope() === 2 ? this.getChipsIds(this.slotChips()) : undefined,
      customSourceTitles: this.slotScope() === 2 ? this.getChipsTitles(this.slotChips()) : undefined
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: r => {
        this.slotResultSig.set(r);
        this.slotLoading.set(false);
        // Let the SlotReelsComponent handle the spinning animation and call onSlotFinished()
      },
      error: e => {
        this.slotLoading.set(false);
        this.slotIsSpinning.set(false);
        this.errorMsg.set(e.error?.error || 'Slot machine failed');
      }
    });
  }

  public onSlotFinished(): void {
    this.slotLoading.set(false);
    this.slotIsSpinning.set(false);
    const r = this.slotResultSig();
    if (r) {
      this.audioService.playSuccess();
      this.checkWinnerAgainstLists(r);
      this.winnerModalMovies.set([r]);
    }
  }

  public generateMysteryBox(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.revealedMovieSig.set(null);
    this.selectedBoxId.set(null);
    this.revealedOthersMap.set({});
    this.mysteryLoading.set(true);
    this.discoveryService.apiV1DiscoveryMysteryBoxPost({
      scope: this.mysteryScope() as any,
      variant: this.mysteryVariant(),
      excludeWatched: this.mysteryExcludeWatched(),
      customSourceIds: this.mysteryScope() === 2 ? this.getChipsIds(this.mysteryChips()) : undefined,
      customSourceTitles: this.mysteryScope() === 2 ? this.getChipsTitles(this.mysteryChips()) : undefined,
      partnerUsername: this.mysteryScope() === 4 ? this.mysteryPartnerUsername() || null : undefined
    }).pipe(
      takeUntil(this.destroy$),
      finalize(() => this.mysteryLoading.set(false))
    )
      .subscribe({ next: r => this.mysteryResultSig.set(r), error: e => this.errorMsg.set(e.error?.error || 'Mystery box failed') });
  }

  public revealBox(boxId: string): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.selectedBoxId.set(boxId);
    this.mysteryRevealing.set(true);
    this.audioService.playFlip();
    
    const movieId = boxId.includes('_') ? boxId.split('_')[1] : boxId;
    this.discoveryService.apiV1DiscoveryMysteryBoxBoxIdRevealGet(movieId)
      .pipe(
        takeUntil(this.destroy$),
        delay(1500),
        finalize(() => this.mysteryRevealing.set(false))
      )
      .subscribe({ 
        next: r => {
          this.revealedMovieSig.set(r);
        }, 
        error: e => this.errorMsg.set(e.error?.error || 'Reveal failed') 
      });
  }

  public revealRemainingMysteryBoxes(): void {
    const box = this.mysteryResultSig();
    const selected = this.selectedBoxId();
    if (!box || !selected) return;

    this.isRevealingOthers.set(true);
    const selectedIndex = selected.includes('_') ? parseInt(selected.split('_')[0], 10) : -1;
    
    const requests = box.boxIds.map((id, index) => {
      if (index === selectedIndex) return null;
      return this.discoveryService.apiV1DiscoveryMysteryBoxBoxIdRevealGet(id);
    });

    const nonNullRequests = requests.filter(r => r !== null) as Observable<SelectionResultDto>[];

    forkJoin(nonNullRequests).pipe(takeUntil(this.destroy$)).subscribe({
      next: results => {
        const newMap: Record<string, SelectionResultDto> = {};
        let resultIdx = 0;
        box.boxIds.forEach((id, index) => {
          if (index !== selectedIndex) {
            newMap[`${index}_${id}`] = results[resultIdx++];
          }
        });
        this.revealedOthersMap.set(newMap);
        this.isRevealingOthers.set(false);
      },
      error: () => {
        this.isRevealingOthers.set(false);
        this.errorMsg.set('Failed to reveal some boxes');
      }
    });
  }

  public onMysteryFinished(): void {
    const r = this.revealedMovieSig();
    if (r) {
      this.audioService.playSuccess();
      this.checkWinnerAgainstLists(r);
      this.winnerModalMovies.set([r]);
    }
  }

  public loadBingo(newBoard: boolean = false): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }

    const currentBoard = this.bingoResultSig();
    if (newBoard && currentBoard && currentBoard.boardId) {
      const savedIds = this.getSavedBoardIds();
      if (!savedIds.includes(currentBoard.boardId)) {
        this.showConfirm(
          'UNSAVED CHALLENGE',
          'You have an active unsaved bingo board. Would you like to save it before generating a new one, or discard it?',
          [
            { label: 'Save & Generate', value: 'save', type: 'primary' },
            { label: 'Discard & Generate', value: 'discard', type: 'danger' },
            { label: 'Cancel', value: 'cancel', type: 'secondary' }
          ]
        ).then((choice) => {
          if (choice === 'cancel') return;
          if (choice === 'save') {
            this.saveBoardId(currentBoard.boardId);
            this.executeLoadBingo(true);
          } else if (choice === 'discard') {
            this.bingoLoading.set(true);
            this.discoveryService.apiV1DiscoveryBingoBoardsBoardIdDelete(currentBoard.boardId)
              .pipe(takeUntil(this.destroy$))
              .subscribe({
                next: () => {
                  this.executeLoadBingo(true);
                },
                error: (e) => {
                  this.bingoLoading.set(false);
                  this.errorMsg.set(e.error?.error || 'Failed to discard current board');
                }
              });
          }
        });
        return;
      }
    }

    this.executeLoadBingo(newBoard);
  }

  public autofillBingo(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.executeLoadBingo(false, true);
  }

  private executeLoadBingo(newBoard: boolean, autoEvaluate: boolean = false): void {
    this.errorMsg.set(null);
    this.bingoLoading.set(true);
    this.discoveryService.apiV1DiscoveryBingoPost({
      gridSize: this.bingoGridSize(),
      scope: this.bingoScope() as any,
      excludeWatched: this.bingoExcludeWatched(),
      customSourceIds: this.bingoScope() === 2 ? this.getChipsIds(this.bingoChips()) : undefined,
      customSourceTitles: this.bingoScope() === 2 ? this.getChipsTitles(this.bingoChips()) : undefined,
      durationDays: newBoard ? (this.bingoDurationDays() !== null ? this.bingoDurationDays()! : -1) : undefined,
      autoEvaluate: autoEvaluate
    })
      .pipe(takeUntil(this.destroy$), finalize(() => this.bingoLoading.set(false)))
      .subscribe({
        next: (r: any) => {
          this.bingoResultSig.set(r);
          if (!newBoard && r && r.boardId) {
            this.saveBoardId(r.boardId);
          }
          this.loadBingoBoards();
        },
        error: (e: any) => this.errorMsg.set(e.error?.error || 'Bingo load failed')
      });
  }

  public openCandidatesModal(square: any): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    if (square.isCompleted) return;
    this.selectedObjectiveId = square.objectiveId;
    this.selectedSquare = square;
    this.showCandidatesModal.set(true);
    this.candidatesLoading.set(true);
    this.discoveryService.apiV1DiscoveryBingoSquaresCandidatesGet(square.objectiveId)
      .pipe(takeUntil(this.destroy$), finalize(() => this.candidatesLoading.set(false)))
      .subscribe({
        next: (candidates) => {
          this.candidateSquares.set(candidates || []);
        },
        error: (e) => {
          this.errorMsg.set(e.error?.error || 'Failed to load candidate movies');
        }
      });
  }

  public claimObjective(candidate: any): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    if (!this.selectedObjectiveId) return;

    this.showConfirm(
      'CONFIRM SELECTION',
      `Once you assign "${candidate.movieTitle}" to this square, it cannot be changed or moved. Do you want to proceed?`,
      [
        { label: 'Cancel', value: 'cancel', type: 'secondary' },
        { label: 'Confirm', value: 'confirm', type: 'primary' }
      ]
    ).then((choice) => {
      if (choice !== 'confirm') return;

      this.bingoLoading.set(true);
      this.discoveryService.apiV1DiscoveryBingoClaimPost({
        objectiveId: this.selectedObjectiveId!,
        diaryEntryId: candidate.diaryEntryId
      })
        .pipe(takeUntil(this.destroy$), finalize(() => this.bingoLoading.set(false)))
        .subscribe({
          next: (r: any) => {
            this.bingoResultSig.set(r);
            this.showCandidatesModal.set(false);
            this.audioService.playSuccess();
            this.loadBingoBoards();
          },
          error: (e) => {
            this.errorMsg.set(e.error?.error || 'Failed to claim bingo objective');
          }
        });
    });
  }

  public get currentDate(): Date {
    return new Date();
  }

  public isBoardExpired(endDate: string | Date | null | undefined): boolean {
    if (!endDate) return false;
    return new Date(endDate) < new Date();
  }

  public isSquareCompleted(s: any): boolean {
    return s.isCompleted;
  }

  public hasCompletedLateSquare(bingo: any): boolean {
    if (!bingo || !bingo.endDate) return false;
    const end = new Date(bingo.endDate);
    return bingo.squares.some((s: any) => s.completionDate && new Date(s.completionDate) > end);
  }

  public loadBingoBoards(): void {
    if (!this.auth.isAuthenticated()) return;
    this.discoveryService.apiV1DiscoveryBingoBoardsGet().pipe(takeUntil(this.destroy$)).subscribe({
      next: (boards) => {
        this.bingoBoards.set(boards || []);
      },
      error: (e) => {
        console.warn('Failed to load user bingo boards.', e);
      }
    });
  }

  public loadSpecificBingoBoard(boardId: string): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.saveBoardId(boardId);
    this.errorMsg.set(null);
    this.bingoLoading.set(true);
    this.discoveryService.apiV1DiscoveryBingoPost({
      gridSize: this.bingoGridSize(),
      scope: this.bingoScope() as any,
      excludeWatched: this.bingoExcludeWatched(),
      boardId: boardId
    })
      .pipe(takeUntil(this.destroy$), finalize(() => this.bingoLoading.set(false)))
      .subscribe({
        next: (r: any) => {
          this.bingoResultSig.set(r);
          if (r.gridSize) {
            this.bingoGridSize.set(r.gridSize);
          }
        },
        error: (e: any) => this.errorMsg.set(e.error?.error || 'Failed to load specific board')
      });
  }

  public deleteBingoBoard(boardId: string): void {
    if (!this.auth.isAuthenticated()) return;
    this.showConfirm(
      'DELETE CHALLENGE',
      'Are you sure you want to delete this bingo board? All progress on this board will be lost.',
      [
        { label: 'Cancel', value: 'cancel', type: 'secondary' },
        { label: 'Delete', value: 'delete', type: 'danger' }
      ]
    ).then((choice) => {
      if (choice !== 'delete') return;

      this.bingoLoading.set(true);
      this.discoveryService.apiV1DiscoveryBingoBoardsBoardIdDelete(boardId)
        .pipe(takeUntil(this.destroy$), finalize(() => this.bingoLoading.set(false)))
        .subscribe({
          next: () => {
            if (this.bingoResultSig()?.boardId === boardId) {
              this.bingoResultSig.set(null);
            }
            this.loadBingoBoards();
          },
          error: (e) => {
            this.errorMsg.set(e.error?.error || 'Failed to delete bingo board');
          }
        });
    });
  }

  public rerollBingoObjective(objectiveId: string): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.bingoLoading.set(true);
    this.discoveryService.apiV1DiscoveryBingoRerollObjectiveIdPost(objectiveId)
      .pipe(takeUntil(this.destroy$), finalize(() => this.bingoLoading.set(false)))
      .subscribe({
        next: (r: BingoGridDto) => {
          this.bingoResultSig.set(r);
        },
        error: (e: any) => {
          this.errorMsg.set(e.error?.error || 'Reroll failed');
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public trackByIndex(index: number): number {
    return index;
  }

  public trackById(_: number, item: { objectiveId?: string; boxId?: string }): string {
    return item.objectiveId ?? item.boxId ?? '';
  }

  public trackByBoardId(_: number, item: BingoBoardDto): string {
    return item.boardId ?? '';
  }

  public trackByToast(_: number, toast: ToastCustomListWinner): string {
    return `${toast.listId}-${toast.movieId}`;
  }

  public onRouletteNicknameMapChange(map: Map<string, string>): void {
    this.rouletteNicknameMap = map;
  }

  public toggleRouletteFullscreen(panelEl: HTMLElement): void {
    if (!document.fullscreenElement) {
      panelEl.requestFullscreen().then(() => {
        this.rouletteIsFullscreen.set(true);
      }).catch(err => console.warn('Fullscreen error:', err));
    } else {
      document.exitFullscreen().then(() => {
        this.rouletteIsFullscreen.set(false);
      });
    }
  }
}
