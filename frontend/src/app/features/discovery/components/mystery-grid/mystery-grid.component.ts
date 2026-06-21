import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MysteryBoxDto } from '../../../../core/api/model/mystery-box-dto';
import { SelectionResultDto } from '../../../../core/api/model/selection-result-dto';

@Component({
  selector: 'app-mystery-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mystery-grid.html',
  styleUrl: './mystery-grid.scss'
})
export class MysteryGridComponent implements OnChanges, OnInit {
  @Input() mysteryBox: MysteryBoxDto | null = null;
  @Input() revealedMovie: SelectionResultDto | null = null;
  @Input() isRevealing: boolean = false;
  @Input() revealedOthers: Record<string, any> = {};

  @Output() boxClicked = new EventEmitter<string>();
  @Output() interact = new EventEmitter<void>();
  @Output() finished = new EventEmitter<void>();

  public selectedBoxId: string | null = null;
  public showRevealed: boolean = false;
  
  public isDummy: boolean = false;

  ngOnInit(): void {
    if (!this.mysteryBox) {
      this.isDummy = true;
      this.mysteryBox = {
        boxIds: ['dummy-1', 'dummy-2', 'dummy-3', 'dummy-4', 'dummy-5']
      } as any;
    }
  }

  public getMovieForBox(boxId: string): any {
    if (this.selectedBoxId === boxId) {
      return this.revealedMovie;
    }
    return this.revealedOthers ? this.revealedOthers[boxId] : null;
  }

  public getBoxColor(index: number): string {
    const colors = [
      'var(--accent-silver)',
      'var(--accent-sepia)',
      'var(--accent-record)',
      'var(--accent-emerald)',
      'var(--accent-silver)'
    ];
    return colors[index % colors.length];
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['mysteryBox'] && this.mysteryBox) {
      this.isDummy = false;
      this.selectedBoxId = null;
      this.showRevealed = false;
    }
    if (this.revealedMovie && !this.isRevealing && !this.showRevealed) {
      this.showRevealed = true;
      setTimeout(() => {
        this.finished.emit();
      }, 1200); // Wait for the 1.2s explosion animation to complete
    }
  }

  public clickBox(boxId: string): void {
    if (this.isDummy) {
      this.interact.emit();
      return;
    }
    if (this.selectedBoxId || this.isRevealing || this.showRevealed) return;
    this.selectedBoxId = boxId;
    this.boxClicked.emit(boxId);
  }
}
