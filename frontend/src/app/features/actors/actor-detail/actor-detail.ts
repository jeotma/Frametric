import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ActorsService, ActorDetailsDto } from '../../../core/api';

@Component({
  selector: 'app-actor-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './actor-detail.html',
  styleUrl: './actor-detail.scss'
})
export class ActorDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private actorsService = inject(ActorsService);

  actor = signal<ActorDetailsDto | null>(null);
  isLoading = signal(true);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.actorsService.apiActorsIdGet(id).subscribe({
        next: (data) => {
          this.actor.set(data);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        }
      });
    }
  }
}
