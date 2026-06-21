import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-cinematic-tooltip',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="tooltip-container">
      <svg class="info-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="12" cy="12" r="10"></circle>
        <line x1="12" y1="16" x2="12" y2="12"></line>
        <line x1="12" y1="8" x2="12.01" y2="8"></line>
      </svg>
      <div class="tooltip-content" [class.tooltip-wide]="wide">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: []
})
export class CinematicTooltipComponent {
  @Input() wide: boolean = false;
}
