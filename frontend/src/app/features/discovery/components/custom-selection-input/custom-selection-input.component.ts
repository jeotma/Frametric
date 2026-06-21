import { Component, EventEmitter, Input, Output, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, finalize, tap } from 'rxjs';
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
export class CustomSelectionInputComponent {
  private searchService = inject(SearchService);
  private moviesService = inject(MoviesService);
  private customListsService = inject(CustomListsService);
  private auth = inject(AuthService);

  @Input() selectedChips: MovieSimpleDto[] = [];
  @Output() selectedChipsChange = new EventEmitter<MovieSimpleDto[]>();

  public searchQuery = signal<string>('');
  public searchResults = signal<GlobalSearchResultDto[]>([]);
  public isSearching = signal<boolean>(false);
  public showDropdown = signal<boolean>(false);
  public enrichLoadingId = signal<number | null>(null);

  public savedLists = signal<CustomListDto[]>([]);
  public showSaveModal = signal<boolean>(false);
  public newListName = signal<string>('');

  private searchSubject = new Subject<string>();

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
      })
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
        .pipe(finalize(() => this.enrichLoadingId.set(null)))
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
    this.selectedChips = this.selectedChips.filter((_, i) => i !== index);
    this.selectedChipsChange.emit(this.selectedChips);
  }

  private clearSearch(): void {
    this.searchQuery.set('');
    this.searchResults.set([]);
    this.showDropdown.set(false);
  }

  // --- Saved Lists Logic ---

  private loadSavedLists(): void {
    this.customListsService.apiV1CustomListsGet().subscribe({
      next: lists => this.savedLists.set(lists),
      error: () => console.error('Failed to load custom lists')
    });
  }

  public loadList(list: CustomListDto): void {
    if (list.movies) {
      this.selectedChips = [...list.movies].slice(0, 50);
      this.selectedChipsChange.emit(this.selectedChips);
    }
  }

  public openSaveModal(): void {
    if (this.selectedChips.length === 0) return;
    this.newListName.set('');
    this.showSaveModal.set(true);
  }

  public saveCurrentList(): void {
    if (!this.newListName().trim() || this.selectedChips.length === 0) return;

    this.customListsService.apiV1CustomListsPost({
      name: this.newListName(),
      movieIds: this.selectedChips.map(c => c.id!).filter(id => !!id)
    }).subscribe({
      next: (newList) => {
        this.savedLists.update(lists => [newList, ...lists]);
        this.showSaveModal.set(false);
      },
      error: (err) => alert('Could not save list: ' + err.message)
    });
  }
}
