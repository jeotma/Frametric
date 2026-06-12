import { Component, signal, OnInit, OnDestroy, inject, computed } from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ModalService } from '../../core/services/modal.service';
import { CinematicSelectComponent } from '../cinematic-select/cinematic-select.component';
@Component({
  selector: 'app-final-cut-teaser',
  standalone: true,
  imports: [CommonModule, FormsModule, CinematicSelectComponent],
  templateUrl: './final-cut-teaser.html',
  styleUrl: './final-cut-teaser.scss',
})
export class FinalCutTeaserComponent implements OnInit, OnDestroy {
  public selectedYear = signal<number | 'global'>(new Date().getFullYear() - 1);
  public availableYears = [2026, 2025, 2024];

  public yearOptions = computed(() => {
    return [
      { value: 'global', label: 'All-Time' },
      ...this.availableYears.map(y => ({ value: y, label: y.toString() }))
    ];
  });

  private authService = inject(AuthService);
  private modalService = inject(ModalService);
  private router = inject(Router);
  public username = computed(() => this.authService.currentUser()?.username || null);

  public isPlayingPromo = signal<boolean>(true);
  public promoStep = signal<number>(0);
  private promoTimeout: any;

  ngOnInit() {
    this.startPromoLoop();
  }

  ngOnDestroy() {
    this.stopPromoLoop();
  }

  onYearChange(val: any) {
    this.selectedYear.set(val === 'global' ? 'global' : Number(val));
  }

  public togglePromo() {
    if (this.isPlayingPromo()) {
      this.isPlayingPromo.set(false);
      this.stopPromoLoop();
    } else {
      this.isPlayingPromo.set(true);
      this.startPromoLoop();
    }
  }

  private startPromoLoop() {
    this.promoStep.set(0);
    this.queueNextStep();
  }

  private queueNextStep() {
    const currentStep = this.promoStep();
    const duration = currentStep === 4 ? 10000 : 2800;

    this.promoTimeout = setTimeout(() => {
      this.promoStep.update(s => (s + 1) % 5);
      this.queueNextStep();
    }, duration);
  }

  private stopPromoLoop() {
    if (this.promoTimeout) {
      clearTimeout(this.promoTimeout);
      this.promoTimeout = null;
    }
  }

  public goToFinalCut() {
    if (!this.authService.isAuthenticated()) {
      this.modalService.openAuthModal();
      return;
    }
    this.router.navigate(['/final-cut', this.selectedYear()]);
  }
}
