// models/photographer-booking.models.ts

export interface Photographer {
  name: string;
  loc: string;
  type: string;
  tags: string[];
  price: number;
  rating: number;
  reviews: number;
  img: string;
}

export interface Category {
  id: number;
  name: string;
}

export interface SpecialtyTag {
  id: number;
  catId: number;
  name: string;
}

export interface SearchFilters {
  dateRange?: { start: Date | null; end: Date | null };
  serviceType: string;
  keyword: string;
  locations: string[];
  tags: string[];
  maxPrice: number;
  minRating: number;
  sortOrder: 'default' | 'priceAsc' | 'priceDesc' | 'ratingDesc';
}
