import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './star-rating.component.html',
  styleUrl: './star-rating.component.css'
})
export class StarRatingComponent {
  /** Current value 1-5, or null/undefined for no selection */
  value = input<number | null | undefined>(null);
  /** Whether the control is disabled (read-only) */
  disabled = input<boolean>(false);
  /** Max stars (default 5) */
  maxStars = input<number>(5);

  valueChange = output<number | null>();

  get stars(): number[] {
    return Array.from({ length: this.maxStars() }, (_, i) => i + 1);
  }

  get currentValue(): number | null | undefined {
    return this.value();
  }

  select(star: number): void {
    if (this.disabled()) return;
    const v = this.value();
    this.valueChange.emit(v === star ? null : star);
  }

  isFilled(star: number): boolean {
    const v = this.currentValue;
    return v != null && star <= v;
  }
}
