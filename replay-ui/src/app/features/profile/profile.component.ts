import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, UserInfo } from '../../core/services/auth.service';
import { RatingService, RatingDto } from '../../core/services/rating.service';
import { TradeService, TradeDto, TradeQueryParams } from '../../core/services/trade.service';
import { ToyService } from '../../core/services/toy.service';
import { StarRatingComponent } from '../../shared/components/star-rating/star-rating.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, StarRatingComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  ratings = signal<RatingDto[]>([]);
  reputationScore = signal<number | null>(null);
  ratingsLoading = signal(true);

  recentTrades = signal<TradeDto[]>([]);
  tradesLoading = signal(true);

  // Edit mode
  editing = signal(false);
  editName = '';
  saving = signal(false);
  saveError = signal<string | null>(null);
  saveSuccess = signal<string | null>(null);

  // Image upload
  uploadingImage = signal(false);
  imageError = signal<string | null>(null);

  roundedReputation(): number | null {
    const s = this.reputationScore();
    return s != null ? Math.round(s) : null;
  }

  constructor(
    public authService: AuthService,
    private ratingService: RatingService,
    public tradeService: TradeService,
    public toyService: ToyService
  ) {}

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (user?.id) {
      this.ratingService.getUserReputation(user.id).subscribe({
        next: (res) => this.reputationScore.set(res.reputationScore),
        error: () => this.reputationScore.set(null)
      });
      this.ratingService.getUserRatings(user.id).subscribe({
        next: (list) => {
          this.ratings.set(list);
          this.ratingsLoading.set(false);
        },
        error: () => this.ratingsLoading.set(false)
      });
      this.loadRecentTrades();
    } else {
      this.ratingsLoading.set(false);
      this.tradesLoading.set(false);
    }
  }

  loadRecentTrades(): void {
    this.tradesLoading.set(true);
    const params: TradeQueryParams = { pageNumber: 1, pageSize: 5 };
    this.tradeService.getUserTrades(params).subscribe({
      next: (result) => {
        this.recentTrades.set(result.items);
        this.tradesLoading.set(false);
      },
      error: () => this.tradesLoading.set(false)
    });
  }

  getProfileImageUrl(): string | null {
    return this.authService.getProfileImageUrl(this.authService.currentUser()?.profileImageUrl);
  }

  startEditing(): void {
    this.editName = this.authService.currentUser()?.fullName ?? '';
    this.saveError.set(null);
    this.saveSuccess.set(null);
    this.editing.set(true);
  }

  cancelEditing(): void {
    this.editing.set(false);
    this.saveError.set(null);
  }

  saveProfile(): void {
    if (!this.editName.trim()) {
      this.saveError.set('Full name is required.');
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);

    this.authService.updateProfile(this.editName.trim()).subscribe({
      next: (result) => {
        this.saving.set(false);
        if (result.succeeded) {
          this.editing.set(false);
          this.saveSuccess.set('Profile updated successfully.');
          this.clearSuccessAfterDelay();
        } else {
          this.saveError.set(result.message || 'Failed to update profile.');
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.saveError.set(err.error?.message || 'Failed to update profile.');
      }
    });
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];

    // Client-side validation
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.imageError.set('Only JPG, PNG, and WebP images are allowed.');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.imageError.set('Image must be less than 5MB.');
      return;
    }

    this.uploadingImage.set(true);
    this.imageError.set(null);

    this.authService.uploadProfileImage(file).subscribe({
      next: (result) => {
        this.uploadingImage.set(false);
        if (result.succeeded) {
          this.saveSuccess.set('Profile image updated.');
          this.clearSuccessAfterDelay();
        } else {
          this.imageError.set(result.message || 'Failed to upload image.');
        }
      },
      error: (err) => {
        this.uploadingImage.set(false);
        this.imageError.set(err.error?.message || 'Failed to upload image.');
      }
    });

    // Reset the input so the same file can be re-selected
    input.value = '';
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  private clearSuccessAfterDelay(): void {
    setTimeout(() => this.saveSuccess.set(null), 3000);
  }
}
