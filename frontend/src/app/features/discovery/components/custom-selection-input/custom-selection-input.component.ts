import { Component, EventEmitter, Input, Output, OnDestroy, inject, signal, effect, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, finalize, tap } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SearchService } from '../../../../core/api/api/search.service';
import { MoviesService } from '../../../../core/api/api/movies.service';
import { CustomListsService } from '../../../../core/api/api/custom-lists.service';
import { GlobalSearchResultDto } from '../../../../core/api/model/global-search-result-dto';
import { MovieSimpleDto } from '../../../../core/api/model/movie-simple-dto';
import { CustomListDto } from '../../../../core/api/model/custom-list-dto';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-custom-selection-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './custom-selection-input.component.html',
  styleUrl: './custom-selection-input.component.scss'
})
export class CustomSelectionInputComponent implements OnDestroy {
  private searchService = inject(SearchService);
  private moviesService = inject(MoviesService);
  private customListsService = inject(CustomListsService);
  private auth = inject(AuthService);

  @Input() selectedChips: MovieSimpleDto[] = [];
  @Output() selectedChipsChange = new EventEmitter<MovieSimpleDto[]>();

  /** Map of movieId -> custom alias/nickname. Emitted alongside chips so parent can pass to API. */
  public nicknameMap: Map<string, string> = new Map();
  @Output() nicknameMapChange = new EventEmitter<Map<string, string>>();

  public searchQuery = signal<string>('');
  public searchResults = signal<GlobalSearchResultDto[]>([]);
  public isSearching = signal<boolean>(false);
  public showDropdown = signal<boolean>(false);
  public enrichLoadingId = signal<number | null>(null);

  /** Track which chip is being edited for its nickname */
  public editingNicknameIndex = signal<number | null>(null);
  public editingNicknameValue = signal<string>('');

  public savedLists = signal<CustomListDto[]>([]);
  public showSaveModal = signal<boolean>(false);
  public newListName = signal<string>('');

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor() {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => this.isSearching.set(true)),
      switchMap(query => {
        if (!query || query.trim() === '') {
          this.isSearching.set(false);
          return of([]);
        }
        return this.searchService.apiSearchGet(query).pipe(
          finalize(() => this.isSearching.set(false))
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (results) => {
        // Filter out non-movies
        const movies = results.filter(r => r.entityType === 'Movie' || r.entityType === 'tmdb_movie');
        this.searchResults.set(movies);
        this.showDropdown.set(movies.length > 0);
      },
      error: () => this.searchResults.set([])
    });

    effect(() => {
      if (this.auth.isAuthenticated()) {
        this.loadSavedLists();
      }
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
    this.searchSubject.next(value);
  }

  public selectResult(result: GlobalSearchResultDto): void {
    if (this.selectedChips.length >= 50) {
      alert('You can only add up to 50 movies per selection to respect API limits.');
      return;
    }

    if (this.selectedChips.find(c => c.id === result.localId || (c as any).tmdbId === result.tmdbId)) {
      this.clearSearch();
      return; // Already added
    }

    if (result.isLocal && result.localId) {
      this.addChip({
        id: result.localId,
        title: result.titleOrName,
        releaseYear: result.releaseYear,
        posterPath: result.imageUrl
      });
      this.clearSearch();
    } else if (result.tmdbId) {
      this.enrichLoadingId.set(result.tmdbId);
      this.moviesService.apiMoviesEnrichFromTmdbPost({ tmdbId: result.tmdbId })
        .pipe(takeUntil(this.destroy$), finalize(() => this.enrichLoadingId.set(null)))
        .subscribe({
          next: (movie) => {
            this.addChip(movie);
            this.clearSearch();
          },
          error: (err) => {
            alert('Could not enrich movie data. ' + (err.error?.error || ''));
            this.clearSearch();
          }
        });
    }
  }

  private addChip(movie: MovieSimpleDto): void {
    this.selectedChips = [...this.selectedChips, movie];
    this.selectedChipsChange.emit(this.selectedChips);
  }

  public removeChip(index: number): void {
    const chip = this.selectedChips[index];
    if (chip?.id) {
      this.nicknameMap.delete(chip.id);
      this.nicknameMapChange.emit(new Map(this.nicknameMap));
    }
    this.selectedChips = this.selectedChips.filter((_, i) => i !== index);
    this.selectedChipsChange.emit(this.selectedChips);
    if (this.editingNicknameIndex() === index) {
      this.editingNicknameIndex.set(null);
    }
  }

  // --- Nickname editing ---

  public startEditNickname(index: number): void {
    const chip = this.selectedChips[index];
    const existing = chip?.id ? (this.nicknameMap.get(chip.id) ?? chip.nickname ?? '') : '';
    this.editingNicknameIndex.set(index);
    this.editingNicknameValue.set(existing);
  }

  public confirmEditNickname(index: number): void {
    const chip = this.selectedChips[index];
    if (!chip?.id) { this.editingNicknameIndex.set(null); return; }
    const val = this.editingNicknameValue().trim();
    if (val) {
      this.nicknameMap.set(chip.id, val);
    } else {
      this.nicknameMap.delete(chip.id);
    }
    this.nicknameMapChange.emit(new Map(this.nicknameMap));
    this.editingNicknameIndex.set(null);
  }

  public cancelEditNickname(): void {
    this.editingNicknameIndex.set(null);
  }

  public getNicknameForChip(chip: MovieSimpleDto): string | null {
    if (!chip.id) return null;
    return this.nicknameMap.get(chip.id) ?? chip.nickname ?? null;
  }

  private clearSearch(): void {
    this.searchQuery.set('');
    this.searchResults.set([]);
    this.showDropdown.set(false);
  }

  // --- Saved Lists Logic ---

  private loadSavedLists(): void {
    this.customListsService.apiV1CustomListsGet().pipe(takeUntil(this.destroy$)).subscribe({
      next: lists => this.savedLists.set(lists),
      error: () => console.error('Failed to load custom lists')
    });
  }

  public loadList(list: CustomListDto): void {
    if (list.movies) {
      this.selectedChips = [...list.movies].slice(0, 50);
      // Restore nicknames from saved list
      this.nicknameMap = new Map();
      for (const m of this.selectedChips) {
        if (m.id && m.nickname) {
          this.nicknameMap.set(m.id, m.nickname);
        }
      }
      this.selectedChipsChange.emit(this.selectedChips);
      this.nicknameMapChange.emit(new Map(this.nicknameMap));
    }
  }

  public openSaveModal(): void {
    if (this.selectedChips.length === 0) return;
    this.newListName.set('');
    this.showSaveModal.set(true);
  }

  public saveCurrentList(): void {
    if (!this.newListName().trim() || this.selectedChips.length === 0) return;

    const items = this.selectedChips
      .filter(c => !!c.id)
      .map(c => ({
        movieId: c.id!,
        nickname: c.id ? (this.nicknameMap.get(c.id) ?? null) : null
      }));

    this.customListsService.apiV1CustomListsPost({
      name: this.newListName(),
      items: items
    } as any).pipe(takeUntil(this.destroy$)).subscribe({
      next: (newList) => {
        this.savedLists.update(lists => [newList, ...lists]);
        this.showSaveModal.set(false);
      },
      error: (err) => alert('Could not save list: ' + err.message)
    });
  }

  @HostListener('window:keydown.escape', ['$event'])
  handleEscapeKey(event: any) {
    if (this.showSaveModal()) {
      this.showSaveModal.set(false);
    }
  }
}
