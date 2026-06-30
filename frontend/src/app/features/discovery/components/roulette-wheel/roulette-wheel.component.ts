import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ElementRef, ViewChild, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MovieSimpleDto } from '../../../../core/api/model/movie-simple-dto';
import { DiscoveryAudioService } from '../../services/discovery-audio.service';

@Component({
  selector: 'app-roulette-wheel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './roulette-wheel.html',
  styleUrl: './roulette-wheel.scss'
})
export class RouletteWheelComponent implements OnChanges, OnInit, OnDestroy {
  @Input() sequence: MovieSimpleDto[] = [];
  @Input() winnerTitle: string = '';
  @Input() isSpinning: boolean = false;
  @Input() isFinalSpin: boolean = true;
  
  @Output() finished = new EventEmitter<void>();
  @Output() interact = new EventEmitter<void>();

  private audioService = inject(DiscoveryAudioService);

  @ViewChild('wheelGroup', { static: false }) wheelGroupRef!: ElementRef<SVGElement>;
  @ViewChild('pointerGroup', { static: false }) pointerGroupRef!: ElementRef<SVGElement>;
  @ViewChild('shineOverlay', { static: false }) shineOverlayRef!: ElementRef<SVGElement>;

  public slices: string[] = [];
  public sectorPaths: string[] = [];
  public sliceAngle: number = 0;
  public currentRotation: number = 0;
  public transitionStyle: string = 'none';

  private animationFrameId?: number;

  // Elegant dark palette derived from the branding rules
  public colors = [
    '#59595cff', // Vibrant Silver
    '#947b4cff', // Vibrant Sepia
    '#92272cff', // Vibrant Record
    '#1e6248ff'  // Vibrant Emerald
  ];

  ngOnInit(): void {
    if (this.slices.length === 0) {
      this.buildIdleWheel();
    }
  }

  ngOnDestroy(): void {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
  }

  public onWheelClick(): void {
    if (!this.isSpinning) {
      this.interact.emit();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isSpinning'] && this.isSpinning) {
      this.prepareWheel();
      this.spinWheel();
    } else if (changes['winnerTitle'] && this.isSpinning && !changes['winnerTitle'].firstChange) {
      this.spinWheel();
    } else if (changes['isSpinning'] && !this.isSpinning && this.sequence.length === 0) {
      if (this.animationFrameId) {
        cancelAnimationFrame(this.animationFrameId);
        this.animationFrameId = undefined;
      }
      this.currentRotation = 0;
      this.transitionStyle = 'none';
      if (this.wheelGroupRef) {
        const wheelEl = this.wheelGroupRef.nativeElement;
        wheelEl.style.transition = 'none';
        wheelEl.style.transform = 'rotate(0deg)';
      }
      if (this.pointerGroupRef) {
        const pointerEl = this.pointerGroupRef.nativeElement;
        pointerEl.style.transform = 'rotate(0deg)';
      }
      if (this.shineOverlayRef) {
        this.shineOverlayRef.nativeElement.style.transform = 'rotate(0deg)';
      }
      this.buildIdleWheel();
    }
  }

  private buildIdleWheel(): void {
    this.slices = Array(12).fill('SPIN');
    this.sliceAngle = 360 / this.slices.length;
    this.generateSectorPaths();
  }

  private prepareWheel(): void {
    const uniqueTitles = new Set<string>();
    if (this.winnerTitle) uniqueTitles.add(this.winnerTitle);

    // First add all sequence movies to guarantee their presence
    for (const m of this.sequence) {
      if (m?.title) {
        uniqueTitles.add(m.title);
      }
    }

    // Fallback/fill using sequence backwards or other titles if needed
    for (let i = this.sequence.length - 1; i >= 0; i--) {
      if (this.sequence[i]?.title) {
        uniqueTitles.add(this.sequence[i].title as string);
      }
      if (uniqueTitles.size >= 50) break;
    }

    this.slices = Array.from(uniqueTitles);
    this.slices.sort(() => Math.random() - 0.5);

    this.sliceAngle = 360 / this.slices.length;
    this.generateSectorPaths();
  }

  public generateSectorPaths(): void {
    this.sectorPaths = [];
    const radius = 176;
    for (let i = 0; i < this.slices.length; i++) {
      const startAngle = i * this.sliceAngle;
      const endAngle = startAngle + this.sliceAngle;
      
      const startRad = (startAngle * Math.PI) / 180;
      const endRad = (endAngle * Math.PI) / 180;
      
      const x1 = 200 + radius * Math.cos(startRad);
      const y1 = 200 + radius * Math.sin(startRad);
      const x2 = 200 + radius * Math.cos(endRad);
      const y2 = 200 + radius * Math.sin(endRad);
      
      const path = `M 200 200 L ${x1.toFixed(1)} ${y1.toFixed(1)} A ${radius} ${radius} 0 0 1 ${x2.toFixed(1)} ${y2.toFixed(1)} Z`;
      this.sectorPaths.push(path);
    }
  }

  public getFontSize(): string {
    const count = this.slices.length;
    if (count <= 12) return '11px';
    if (count <= 24) return '9px';
    if (count <= 36) return '7.5px';
    return '6.5px';
  }

  public truncateTitle(title: string): string {
    if (!title) return '';
    const count = this.slices.length;
    const maxLen = count <= 12 ? 26 : count <= 24 ? 20 : 16;
    if (title.length > maxLen) {
      return title.substring(0, maxLen - 2) + '..';
    }
    return title;
  }

  private spinWheel(): void {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }

    const winnerIndex = this.slices.indexOf(this.winnerTitle);
    if (winnerIndex === -1 || !this.wheelGroupRef) return;

    const sliceCenterAngle = (winnerIndex * this.sliceAngle) + (this.sliceAngle / 2);
    // Reduced number of spins to keep the peak velocity below the stroboscopic limit (preventing the Wagon-wheel optical illusion)
    const spins = this.isFinalSpin ? (7 + Math.floor(Math.random() * 3)) : (3 + Math.floor(Math.random() * 2));
    const randomOffset = (Math.random() - 0.5) * (this.sliceAngle * 0.7);

    // Calculate target rotation angle cumulatively (always clockwise)
    const targetAngle = (720 - sliceCenterAngle - randomOffset) % 360;
    const currentMod = this.currentRotation % 360;
    let angleDiff = targetAngle - currentMod;
    if (angleDiff <= 0) {
      angleDiff += 360;
    }
    
    const startRotation = this.currentRotation;
    const targetRotation = this.currentRotation + (spins * 360) + angleDiff;

    const baseDurationMs = this.isFinalSpin ? 7000 : 2500;
    const wheelEl = this.wheelGroupRef.nativeElement;
    const pointerEl = this.pointerGroupRef?.nativeElement;

    // Remove any CSS transitions so JS animation handles each frame directly
    wheelEl.style.transition = 'none';
    this.transitionStyle = 'none';
    if (pointerEl) {
      pointerEl.style.transition = 'none';
    }

    // Piecewise Ease-In-Out Physics:
    // Smoothens initial acceleration (quadratic) to prevent aliasing/stroboscopic effect, 
    // and implements organic deceleration (quintic/cubic).
    const ease = (x: number): number => {
      if (this.isFinalSpin) {
        // 15% acceleration (quadratic), 85% deceleration (quintic)
        const p = 0.15;
        const C = 13.603;
        const D = 1.5638;
        return x < p ? (C * x * x) : (1 - D * Math.pow(1 - x, 5));
      } else {
        // 20% acceleration (quadratic), 80% deceleration (cubic)
        const p = 0.2;
        const C = 6.818;
        const D = 1.42045;
        return x < p ? (C * x * x) : (1 - D * Math.pow(1 - x, 3));
      }
    };

    const startTime = performance.now();
    let lastSectorIndex = Math.floor((720 - (startRotation % 360)) % 360 / this.sliceAngle) % this.slices.length;
    let lastAngle = startRotation;
    let pointerRotation = 0;
    let pointerVelocity = 0;
    const springTension = 0.55;  // Coefficient for return force
    const springDamping = 0.72;  // Velocity friction decay (oscillates nicely)

    const animate = (now: number) => {
      const elapsed = now - startTime;
      const progress = Math.min(elapsed / baseDurationMs, 1);
      
      const easeProgress = ease(progress);
      const angle = startRotation + (targetRotation - startRotation) * easeProgress;
      
      const velocity = Math.abs(angle - lastAngle);
      lastAngle = angle;

      // Apply dynamic hardware-accelerated motion blur proportional to rotation velocity
      const maxBlur = 0.35; // Minimal blur to blend edges and prevent flickering/stroboscopic jumping
      const blurVal = Math.min(velocity * 0.012, maxBlur);
      if (blurVal > 0.12) {
        wheelEl.style.filter = `blur(${blurVal.toFixed(2)}px)`;
      } else {
        wheelEl.style.filter = 'none';
      }

      // Force GPU layer composition with translate3d for subpixel smooth rendering
      wheelEl.style.transform = `translate3d(0, 0, 0) rotate(${angle.toFixed(3)}deg)`;

      // Rotate metallic shine reflection very slowly in the same direction to simulate realistic light reflections
      if (this.shineOverlayRef) {
        const shineEl = this.shineOverlayRef.nativeElement;
        const shineAngle = angle * 0.08;
        shineEl.style.transform = `translate3d(0, 0, 0) rotate(${shineAngle.toFixed(3)}deg)`;
      }

      // Calculate pointer ticks based on sector crossings
      const currentSectorIndex = Math.floor((720 - (angle % 360)) % 360 / this.sliceAngle) % this.slices.length;
      if (currentSectorIndex !== lastSectorIndex) {
        // Kick pointer back dynamically (subtle kick force so it doesn't jitter)
        const kickForce = Math.min(4 + velocity * 0.12, 8);
        pointerVelocity = -kickForce;
        lastSectorIndex = currentSectorIndex;
        // Play pointer click/tick sound!
        const speedFactor = Math.min(velocity / 12, 1.0);
        this.audioService.playTick(0.85 + speedFactor * 0.3, 0.25 * speedFactor);
      }

      // Spring-mass-damper physics simulation:
      const force = -springTension * pointerRotation;
      pointerVelocity += force;
      pointerVelocity *= springDamping;
      pointerRotation += pointerVelocity;

      if (pointerEl) {
        pointerEl.style.transform = `translate3d(0, 0, 0) rotate(${pointerRotation.toFixed(2)}deg)`;
      }
      
      if (progress < 1) {
        this.animationFrameId = requestAnimationFrame(animate);
      } else {
        this.currentRotation = targetRotation;
        this.animationFrameId = undefined;
        wheelEl.style.filter = 'none';
        if (pointerEl) {
          pointerEl.style.transform = 'translate3d(0, 0, 0) rotate(0deg)';
        }
        if (this.shineOverlayRef) {
          this.shineOverlayRef.nativeElement.style.transform = 'translate3d(0, 0, 0) rotate(0deg)';
        }
        if (this.isSpinning) {
          this.finished.emit();
        }
      }
    };

    this.animationFrameId = requestAnimationFrame(animate);
  }
}
