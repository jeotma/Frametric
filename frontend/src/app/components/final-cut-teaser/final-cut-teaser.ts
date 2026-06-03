import { Component, signal, OnInit, OnDestroy, inject, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
@Component({
  selector: 'app-final-cut-teaser',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './final-cut-teaser.html',
  styleUrl: './final-cut-teaser.scss',
})
export class FinalCutTeaserComponent implements OnInit, OnDestroy {
  public selectedYear = signal<number | 'global'>(new Date().getFullYear() - 1);
  public availableYears = [2026, 2025, 2024];

  private authService = inject(AuthService);
  public username = computed(() => this.authService.currentUser()?.username || 'User');

  public isPlayingPromo = signal<boolean>(true);
  public promoStep = signal<number>(0);
  private promoTimeout: any;

  ngOnInit() {
    this.startPromoLoop();
  }

  ngOnDestroy() {
    this.stopPromoLoop();
  }

  onYearChange(event: any) {
    const val = event.target.value;
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
    // Step 4 is the final screen. Wait 10 seconds before restarting. Others take 2.8s.
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
}
