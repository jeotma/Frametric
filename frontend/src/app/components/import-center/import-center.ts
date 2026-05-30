import { Component, OnInit, OnDestroy, signal, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImportService } from '../../core/api/api/import.service';
import { ImportHistoryDto } from '../../core/api/model/import-history-dto';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-import-center',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './import-center.html',
  styleUrl: './import-center.scss',
})
export class ImportCenterComponent implements OnInit, OnDestroy {
  private importService = inject(ImportService);
  
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  // State Signals
  public history = signal<ImportHistoryDto[]>([]);
  public isUploading = signal(false);
  public isDragging = signal(false);
  public errorMessage = signal<string | null>(null);

  private pollingInterval: any;

  ngOnInit() {
    this.fetchHistory();
    this.startPolling();
  }

  ngOnDestroy() {
    this.stopPolling();
  }

  private startPolling() {
    // Poll every 5 seconds
    this.pollingInterval = setInterval(() => {
      this.fetchHistory();
    }, 5000);
  }

  private stopPolling() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  }

  private fetchHistory() {
    this.importService.apiImportHistoryGet().subscribe({
      next: (data) => this.history.set(data),
      error: (err) => console.error('Failed to fetch import history', err)
    });
  }

  // Drag and Drop Handlers
  public onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  public onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  public onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
    this.errorMessage.set(null);

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
  }

  // Click to Upload Handlers
  public triggerFileInput() {
    this.fileInput.nativeElement.click();
  }

  public onFileSelected(event: Event) {
    this.errorMessage.set(null);
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFile(input.files[0]);
    }
    // Reset input so the same file can be selected again if needed
    input.value = '';
  }

  private handleFile(file: File) {
    if (!file.name.toLowerCase().endsWith('.zip')) {
      this.errorMessage.set('Only .zip files are supported.');
      return;
    }

    this.isUploading.set(true);
    
    // apiImportLetterboxdPost takes a Blob (File extends Blob)
    this.importService.apiImportLetterboxdPost(file).subscribe({
      next: () => {
        this.isUploading.set(false);
        this.fetchHistory();
      },
      error: (err: HttpErrorResponse) => {
        this.isUploading.set(false);
        if (err.status === 400 || err.status === 500) {
          // If the backend returned a specific message, use it, else generic
          const msg = err.error?.message || err.error || 'Invalid zip archive or missing Letterboxd files.';
          this.errorMessage.set(typeof msg === 'string' ? msg : 'Invalid zip archive or missing Letterboxd files.');
        } else {
          this.errorMessage.set('An unexpected error occurred during upload.');
        }
      }
    });
  }

  public deleteImport(id: string) {
    if (!confirm('Are you sure you want to delete this import? All associated movies and ratings will be removed.')) {
      return;
    }

    this.importService.apiImportIdDelete(id).subscribe({
      next: () => this.fetchHistory(),
      error: (err) => console.error('Failed to delete import', err)
    });
  }
}
