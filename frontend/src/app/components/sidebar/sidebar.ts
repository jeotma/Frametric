import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class SidebarComponent {
  // Inputs & Outputs in Angular 19 style (signals)
  public activeTab = input.required<'dashboard' | 'imports' | 'wrapped'>();
  public tabSelected = output<'dashboard' | 'imports' | 'wrapped'>();

  public selectTab(tab: 'dashboard' | 'imports' | 'wrapped') {
    this.tabSelected.emit(tab);
  }
}
