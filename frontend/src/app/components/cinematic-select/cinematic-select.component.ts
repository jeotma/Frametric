import { Component, Input, forwardRef, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-cinematic-select',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CinematicSelectComponent),
      multi: true
    }
  ],
  template: `
    <div class="cinematic-select-container" [class.is-open]="isOpen">
      <div class="select-trigger glass-select" (click)="toggleOpen()">
        <span class="selected-label">{{ selectedLabel || placeholder }}</span>
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="chevron"><polyline points="6 9 12 15 18 9"></polyline></svg>
      </div>

      <div class="select-dropdown" *ngIf="isOpen">
        <div 
          *ngFor="let option of options" 
          class="select-option" 
          [class.selected]="option.value === value"
          (click)="selectOption(option)">
          {{ option.label }}
        </div>
        <div *ngIf="!options || options.length === 0" class="select-option empty">No options</div>
      </div>
    </div>
  `,
  styles: [`
    .cinematic-select-container {
      position: relative;
      display: inline-block;
      min-width: 120px;
    }

    .select-trigger {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      cursor: pointer;
      user-select: none;
      /* Inherit glass-select styles or base inputs */
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      padding: 8px 16px;
      color: var(--text-primary);
      font-size: 0.95rem;
      transition: all 0.3s ease;
    }

    .select-trigger:hover, .is-open .select-trigger {
      background: rgba(255, 255, 255, 0.08);
      border-color: rgba(255, 255, 255, 0.2);
    }

    .chevron {
      transition: transform 0.3s ease;
      opacity: 0.7;
    }

    .is-open .chevron {
      transform: rotate(180deg);
    }

    .select-dropdown {
      position: absolute;
      top: calc(100% + 4px);
      left: 0;
      min-width: 100%;
      background: rgba(10, 10, 10, 0.95);
      backdrop-filter: blur(12px);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.8);
      overflow: hidden;
      z-index: 10000;
      display: flex;
      flex-direction: column;
      max-height: 300px;
      overflow-y: auto;
    }

    /* Scrollbar styling for dropdown */
    .select-dropdown::-webkit-scrollbar {
      width: 6px;
    }
    .select-dropdown::-webkit-scrollbar-track {
      background: rgba(255,255,255,0.02);
    }
    .select-dropdown::-webkit-scrollbar-thumb {
      background: rgba(255,255,255,0.1);
      border-radius: 3px;
    }

    .select-option {
      padding: 10px 16px;
      cursor: pointer;
      font-size: 0.95rem;
      color: rgba(255, 255, 255, 0.8);
      transition: background 0.2s ease, color 0.2s ease;
      white-space: nowrap;
    }

    .select-option:hover {
      background: rgba(255, 255, 255, 0.08);
      color: var(--text-primary);
    }

    .select-option.selected {
      background: rgba(226, 186, 100, 0.1); /* Sepia tint */
      color: var(--accent-sepia);
      font-weight: 500;
    }

    .select-option.empty {
      cursor: default;
      color: rgba(255, 255, 255, 0.4);
      font-style: italic;
    }
    .select-option.empty:hover {
      background: transparent;
    }
  `]
})
export class CinematicSelectComponent implements ControlValueAccessor {
  @Input() options: { value: any, label: string }[] = [];
  @Input() placeholder: string = 'Select...';
  
  value: any = null;
  isOpen: boolean = false;

  onChange: any = () => {};
  onTouched: any = () => {};

  constructor(private eRef: ElementRef) {}

  get selectedLabel(): string {
    const selected = this.options?.find(o => o.value === this.value);
    return selected ? selected.label : '';
  }

  toggleOpen() {
    this.isOpen = !this.isOpen;
  }

  selectOption(option: any) {
    this.value = option.value;
    this.onChange(this.value);
    this.isOpen = false;
  }

  writeValue(val: any): void {
    this.value = val;
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if(!this.eRef.nativeElement.contains(event.target)) {
      this.isOpen = false;
    }
  }
}
