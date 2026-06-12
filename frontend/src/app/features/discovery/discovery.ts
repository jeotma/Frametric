import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DiscoveryService } from '../../core/api/api/discovery.service';
import { SelectionResultDto } from '../../core/api/model/selection-result-dto';
import { MysteryBoxDto } from '../../core/api/model/mystery-box-dto';
import { BingoGridDto } from '../../core/api/model/bingo-grid-dto';
import { DiceRollResultDto } from '../../core/api/model/dice-roll-result-dto';
import { SlotMachineResultDto } from '../../core/api/model/slot-machine-result-dto';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ModalService } from '../../core/services/modal.service';
import { CinematicSelectComponent } from '../../components/cinematic-select/cinematic-select.component';

type DiscoveryTab = 'roulette' | 'dice' | 'slot-machine' | 'mystery-box' | 'bingo';

@Component({
  selector: 'app-discovery',
  standalone: true,
  imports: [CommonModule, FormsModule, CinematicSelectComponent],
  templateUrl: './discovery.html',
  styleUrl: './discovery.scss'
})
export class DiscoveryComponent {
  private discoveryService = inject(DiscoveryService);
  public auth = inject(AuthService);
  public modalService = inject(ModalService);

  public activeTab = signal<DiscoveryTab>('bingo');
  private tabMap: Record<string, DiscoveryTab> = {
    bingo: 'bingo', roulette: 'roulette', 'mystery-box': 'mystery-box',
    dice: 'dice', 'slot-machine': 'slot-machine',
  };

  // Options for Cinematic Selects
  public scopeOptions = [
    { value: 1, label: 'Your Library' },
    { value: 2, label: 'Your Watchlist' },
    { value: 3, label: 'Known Universe' }
  ];
  public diceScopeOptions = [
    { value: 1, label: 'Your Library' },
    { value: 2, label: 'Your Watchlist' }
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
  public rouletteResultSig = signal<SelectionResultDto | null>(null);
  public rouletteLoading = signal(false);
  public rouletteScope = signal<number>(1);
  public rouletteThreshold = signal<number>(0);

  // Dice state
  public diceResultSig = signal<DiceRollResultDto | null>(null);
  public diceLoading = signal(false);
  public diceScope = signal<number>(1);

  // Slot Machine state
  public slotResultSig = signal<SlotMachineResultDto | null>(null);
  public slotLoading = signal(false);
  public slotScope = signal<number>(1);
  public slotGenre = signal<string>('');
  public slotDecadeVal = signal<number | null>(null);

  // Mystery Box state
  public mysteryResultSig = signal<MysteryBoxDto | null>(null);
  public revealedMovieSig = signal<SelectionResultDto | null>(null);
  public mysteryLoading = signal(false);
  public mysteryRevealing = signal(false);
  public mysteryScope = signal<number>(1);
  public mysteryVariant = signal<number>(0);
  public selectedBoxId = signal<string | null>(null);

  // Bingo state
  public bingoResultSig = signal<BingoGridDto | null>(null);
  public bingoLoading = signal(false);
  public bingoGridSize = signal<number>(3);

  public errorMsg = signal<string | null>(null);

  public setActiveTab(tab: string): void {
    const mapped = this.tabMap[tab];
    if (mapped) this.activeTab.set(mapped);
  }

  public spellRoulette(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.rouletteLoading.set(true);
    this.discoveryService.apiV1DiscoveryRoulettePost({
      scope: this.rouletteScope(),
      persistenceThreshold: this.rouletteThreshold() > 1 ? this.rouletteThreshold() : null
    }).pipe(finalize(() => this.rouletteLoading.set(false)))
      .subscribe({ next: r => this.rouletteResultSig.set(r), error: e => this.errorMsg.set(e.error?.error || 'Roulette failed') });
  }

  public rollDice(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.diceLoading.set(true);
    this.discoveryService.apiV1DiscoveryDicePost({ scope: this.diceScope() })
      .pipe(finalize(() => this.diceLoading.set(false)))
      .subscribe({ next: r => this.diceResultSig.set(r), error: e => this.errorMsg.set(e.error?.error || 'Dice roll failed') });
  }

  public spinSlots(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.slotLoading.set(true);
    this.discoveryService.apiV1DiscoverySlotMachinePost({
      scope: this.slotScope(),
      genre: this.slotGenre() || null,
      decade: this.slotDecadeVal()
    }).pipe(finalize(() => this.slotLoading.set(false)))
      .subscribe({ next: r => this.slotResultSig.set(r), error: e => this.errorMsg.set(e.error?.error || 'Slot machine failed') });
  }

  public generateMysteryBox(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.revealedMovieSig.set(null);
    this.selectedBoxId.set(null);
    this.mysteryLoading.set(true);
    this.discoveryService.apiV1DiscoveryMysteryBoxPost({
      scope: this.mysteryScope(),
      variant: this.mysteryVariant(),
    }).pipe(finalize(() => this.mysteryLoading.set(false)))
      .subscribe({ next: r => this.mysteryResultSig.set(r), error: e => this.errorMsg.set(e.error?.error || 'Mystery box failed') });
  }

  public revealBox(boxId: string): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.selectedBoxId.set(boxId);
    this.mysteryRevealing.set(true);
    this.discoveryService.apiV1DiscoveryMysteryBoxBoxIdRevealGet(boxId)
      .pipe(finalize(() => this.mysteryRevealing.set(false)))
      .subscribe({ next: r => this.revealedMovieSig.set(r), error: e => this.errorMsg.set(e.error?.error || 'Reveal failed') });
  }

  public loadBingo(): void {
    if (!this.auth.isAuthenticated()) { this.modalService.openAuthModal(); return; }
    this.errorMsg.set(null);
    this.bingoLoading.set(true);
    this.discoveryService.apiV1DiscoveryBingoGet(this.bingoGridSize())
      .pipe(finalize(() => this.bingoLoading.set(false)))
      .subscribe({ next: r => this.bingoResultSig.set(r), error: e => this.errorMsg.set(e.error?.error || 'Bingo load failed') });
  }

  public trackByIndex(index: number): number {
    return index;
  }

  public trackById(_: number, item: { objectiveId?: string; boxId?: string }): string {
    return item.objectiveId ?? item.boxId ?? '';
  }
}
