import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

interface ImportItem {
  id: number;
  filename: string;
  date: string;
  status: string;
  movies: number;
  progress: number;
}

@Component({
  selector: 'app-import-center',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './import-center.html',
  styleUrl: './import-center.scss',
})
export class ImportCenterComponent {
  public recentImports = input.required<ImportItem[]>();
  public isUploading = input.required<boolean>();
  public uploadProgress = input.required<number>();

  public triggerUpload = output<void>();

  public onUpload() {
    this.triggerUpload.emit();
  }
}
