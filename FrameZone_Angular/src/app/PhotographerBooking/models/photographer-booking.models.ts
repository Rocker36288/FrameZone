// models/photographer-booking.models.ts

export interface PhotographerDto {
  photographerId: number;
  displayName: string;
  studioName: string;
  studioType: string;
  studioAddress: string;
  description: string;
  yearsOfExperience?: number;
  avatarUrl: string;
  portfolioUrl: string;
  specialties: string[];
  services: ServiceDto[];
  // UI helpers
  rating?: number;
  reviewCount?: number;
  minPrice?: number;
  totalBookings?: number;
  portfolioFile?: string;
}

export interface ServiceDto {
  photographerServiceId: number;
  serviceTypeId: number;
  serviceName: string;
  description: string;
  basePrice: number;
  duration: number;
  maxRevisions?: number;
  deliveryDays?: number;
  includedPhotos?: number;
  additionalServices: string;
}

export interface PhotographerSearchDto {
  keyword?: string;
  location?: string;
  studioType?: string;
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

export interface AvailableSlotDto {
  availableSlotId: number;
  startDateTime: string; // ISO string
  endDateTime: string;
  isAvailable: boolean;
}

export interface BookingDto {
  bookingId: number;
  photographerId: number;
  photographerName: string;
  userId: number;
  userName: string;
  bookingStartDatetime: string;
  bookingEndDatetime: string;
  bookingStatus: string;
  servicePrice: number;
  location: string;
}

export interface CreateBookingDto {
  photographerId: number;
  availableSlotId: number;
  userId: number;
  location: string;
  paymentMethodId: number;
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

// Keep old Photographer interface for compatibility if needed, or remove it.
// I'll alias it to PhotographerDto for now to minimize breakage if types are checked by name roughly
export type Photographer = PhotographerDto; 
