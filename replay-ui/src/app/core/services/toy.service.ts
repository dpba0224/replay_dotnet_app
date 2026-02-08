import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Toy {
  id: string;
  name: string;
  description: string;
  category: string;
  ageGroup: string;
  condition: number;
  conditionLabel: string;
  price: number;
  status: string;
  isArchived: boolean;
  shareableSlug: string;
  createdAt: string;
  images: ToyImage[];
}

export interface ToyImage {
  id: string;
  imagePath: string;
  displayOrder: number;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ToyQueryParams {
  searchTerm?: string;
  category?: string;
  minCondition?: number;
  ageGroup?: string;
  status?: string;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  pageNumber?: number;
  pageSize?: number;
  includeArchived?: boolean;
}

export interface CreateToyDto {
  name: string;
  description: string;
  category: number;
  ageGroup: string;
  condition: number;
  price: number;
}

export interface UpdateToyDto {
  name?: string;
  description?: string;
  category?: number;
  ageGroup?: string;
  condition?: number;
  price?: number;
}

export const ToyCategories = [
  { value: 0, label: 'Action Figures & Collectibles' },
  { value: 1, label: 'Building Sets' },
  { value: 2, label: 'Board Games & Puzzles' },
  { value: 3, label: 'Dolls & Plush' },
  { value: 4, label: 'Vehicles & RC' },
  { value: 5, label: 'Educational & STEM' },
  { value: 6, label: 'Outdoor & Sports' },
  { value: 7, label: 'Vintage & Retro' }
];

export const ToyConditions = [
  { value: 1, label: 'Acceptable', description: 'Heavy wear, may have minor damage' },
  { value: 2, label: 'Fair', description: 'Noticeable wear, minor scratches' },
  { value: 3, label: 'Good', description: 'Normal use, minor cosmetic wear' },
  { value: 4, label: 'Excellent', description: 'Very light use, no visible wear' },
  { value: 5, label: 'Mint', description: 'Unused or barely used' }
];

export const AgeGroups = [
  '0-2', '3-5', '6-8', '9-12', '13+', 'All Ages'
];

@Injectable({
  providedIn: 'root'
})
export class ToyService {
  private readonly apiUrl = `${environment.apiUrl}/toys`;

  loading = signal<boolean>(false);

  constructor(private http: HttpClient) {}

  getToys(params: ToyQueryParams = {}): Observable<PagedResult<Toy>> {
    let httpParams = new HttpParams();

    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params.category) httpParams = httpParams.set('category', params.category);
    if (params.minCondition) httpParams = httpParams.set('minCondition', params.minCondition.toString());
    if (params.ageGroup) httpParams = httpParams.set('ageGroup', params.ageGroup);
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.minPrice) httpParams = httpParams.set('minPrice', params.minPrice.toString());
    if (params.maxPrice) httpParams = httpParams.set('maxPrice', params.maxPrice.toString());
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.includeArchived) httpParams = httpParams.set('includeArchived', 'true');

    return this.http.get<PagedResult<Toy>>(this.apiUrl, { params: httpParams });
  }

  getMyToys(): Observable<Toy[]> {
    return this.http.get<Toy[]>(`${this.apiUrl}/mine`);
  }

  getToyById(id: string): Observable<Toy> {
    return this.http.get<Toy>(`${this.apiUrl}/${id}`);
  }

  getToyBySlug(slug: string): Observable<Toy> {
    return this.http.get<Toy>(`${this.apiUrl}/slug/${slug}`);
  }

  createToy(dto: CreateToyDto): Observable<Toy> {
    return this.http.post<Toy>(this.apiUrl, dto);
  }

  updateToy(id: string, dto: UpdateToyDto): Observable<Toy> {
    return this.http.put<Toy>(`${this.apiUrl}/${id}`, dto);
  }

  archiveToy(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/archive`, {});
  }

  restoreToy(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/restore`, {});
  }

  uploadImage(toyId: string, file: File, displayOrder: number = 1): Observable<ToyImage> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ToyImage>(`${this.apiUrl}/${toyId}/images?displayOrder=${displayOrder}`, formData);
  }

  deleteImage(toyId: string, imageId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${toyId}/images/${imageId}`);
  }

  getImageUrl(imagePath: string): string {
    if (!imagePath) return '/assets/placeholder-toy.png';
    if (imagePath.startsWith('http')) return imagePath;
    return `${environment.apiUrl.replace('/api/v1', '')}/${imagePath}`;
  }

  getCategoryLabel(category: string): string {
    const cat = ToyCategories.find(c => c.label.replace(/\s+&\s+/g, 'And').replace(/\s+/g, '') === category);
    return cat?.label || category;
  }

  getConditionLabel(condition: number): string {
    const cond = ToyConditions.find(c => c.value === condition);
    return cond?.label || `Condition ${condition}`;
  }
}
