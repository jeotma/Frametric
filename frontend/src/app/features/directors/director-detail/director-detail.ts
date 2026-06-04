import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DirectorsService, DirectorDetailsDto } from '../../../core/api';

@Component({
  selector: 'app-director-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './director-detail.html',
  styleUrl: './director-detail.scss'
})
export class DirectorDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private directorsService = inject(DirectorsService);

  director = signal<DirectorDetailsDto | null>(null);
  isLoading = signal(true);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.directorsService.apiDirectorsIdGet(id).subscribe({
        next: (data) => {
          this.director.set(data);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        }
      });
    }
  }
}
