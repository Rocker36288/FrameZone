import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, map } from 'rxjs';
import {
  Category,
  PhotographerDto,
  SearchFilters,
  SpecialtyTag,
  AvailableSlotDto,
  BookingDto,
  CreateBookingDto,
  PhotographerSearchDto,
  CategoryWithTags,
  ServiceType
} from '../models/photographer-booking.models';


@Injectable({
  providedIn: 'root',
})
export class PhotographerBookingService {
  private apiUrl = `https://localhost:7213/api/Photographer`; // Adjust base URL as needed
  private bookingUrl = `https://localhost:7213/api/Booking`;

  // Filters state
  private filtersSubject = new BehaviorSubject<SearchFilters>({
    serviceType: '',
    keyword: '',
    locations: [],
    tags: [],
    maxPrice: 10000,
    minRating: 0,
    sortOrder: 'default',
  });

  filters$ = this.filtersSubject.asObservable();

  // Mock data for UI helpers
  private readonly locationList = ['台北', '新北', '桃園', '台中', '台南', '高雄', '基隆', '宜蘭', '新竹'];
  private readonly categories: Category[] = [
    { id: 2, name: '拍攝風格' },
    { id: 3, name: '技術專長' },
  ];
  private readonly specialtyTags: SpecialtyTag[] = [
    { id: 201, catId: 2, name: '黑白風格' },
    { id: 202, catId: 2, name: '日系清新' },
    { id: 203, catId: 2, name: '底片感' },
    { id: 204, catId: 2, name: '美式寫實' },
    { id: 205, catId: 2, name: '韓系簡約' },
    { id: 301, catId: 3, name: '微距拍攝' },
    { id: 302, catId: 3, name: '空拍/航拍' },
    { id: 303, catId: 3, name: '水下攝影' },
    { id: 304, catId: 3, name: '夜景專家' },
  ];
  private readonly serviceTypes = ['人像寫真', '婚禮紀錄', '活動攝影', '商業廣告'];

  constructor(private http: HttpClient) { }

  // --- API Calls ---

  getAllPhotographers(): Observable<PhotographerDto[]> {
    return this.http.get<PhotographerDto[]>(this.apiUrl);
  }

  getPhotographerById(id: number): Observable<PhotographerDto> {
    return this.http.get<PhotographerDto>(`${this.apiUrl}/${id}`);
  }

  // Search using our backend search DTO or query params
  searchPhotographers(searchDto: PhotographerSearchDto): Observable<PhotographerDto[]> {
    let params = new HttpParams();
    if (searchDto.keyword) params = params.set('keyword', searchDto.keyword);
    if (searchDto.location) params = params.set('location', searchDto.location);
    if (searchDto.studioType) params = params.set('studioType', searchDto.studioType);

    return this.http.get<PhotographerDto[]>(this.apiUrl, { params });
  }

  // Search using frontend filters - converting to backend call + client side filtering if needed
  // For now, let's try to map some filters to backend params and do the rest client side if backend is limited
  searchWithFilters(filters: SearchFilters): Observable<PhotographerDto[]> {
    let params = new HttpParams();
    if (filters.keyword) params = params.set('keyword', filters.keyword);
    if (filters.serviceType) params = params.set('studioType', filters.serviceType); // Map to backend param
    if (filters.startDate) params = params.set('startDate', filters.startDate);
    if (filters.endDate) params = params.set('endDate', filters.endDate);

    // 如果只有一個地區,傳給後端
    if (filters.locations.length === 1) {
      params = params.set('location', filters.locations[0]);
    }

    // 如果只有一個標籤,傳給後端
    if (filters.tags.length === 1) {
      params = params.set('tag', filters.tags[0]);
    }

    return this.http.get<PhotographerDto[]>(this.apiUrl, { params }).pipe(
      map(photographers => {
        // Apply client-side filters for multiple selections
        return photographers.filter(p => {
          // 地區篩選: 如果有選擇地區,攝影師的服務城市列表必須包含至少一個選中的地區
          const matchLoc = filters.locations.length === 0 ||
            filters.locations.some(l => p.serviceCities?.includes(l));

          // 標籤篩選: 如果有選擇標籤,攝影師必須擁有至少一個選中的標籤
          const matchTags = filters.tags.length === 0 ||
            filters.tags.some(tag => p.specialties.some(s => s.includes(tag)));

          // 價格篩選
          const matchPrice = !p.minPrice || p.minPrice <= filters.maxPrice;

          // 評分篩選
          // 評分篩選
          const matchRating = !p.rating || p.rating >= filters.minRating;

          // 服務類型篩選 (serviceType 為 ID)
          const matchServiceType = !filters.serviceType ||
            p.services?.some(s => s.serviceTypeId.toString() === filters.serviceType.toString());

          return matchLoc && matchTags && matchPrice && matchRating && matchServiceType;
        });
      }),
      map(photographers => {
        // 實作排序邏輯
        const sortOrder = filters.sortOrder;
        if (!sortOrder || sortOrder === 'default') return photographers;

        return [...photographers].sort((a, b) => {
          switch (sortOrder) {
            case 'priceAsc':
              return (a.minPrice || 0) - (b.minPrice || 0);
            case 'priceDesc':
              return (b.minPrice || 0) - (a.minPrice || 0);
            case 'ratingDesc':
              return (b.rating || 0) - (a.rating || 0);
            default:
              return 0;
          }
        });
      })
    );
  }


  getAvailableSlots(photographerId: number, start: Date, end: Date): Observable<AvailableSlotDto[]> {
    let params = new HttpParams()
      .set('start', start.toISOString())
      .set('end', end.toISOString());
    return this.http.get<AvailableSlotDto[]>(`${this.apiUrl}/${photographerId}/slots`, { params });
  }

  createBooking(bookingDto: CreateBookingDto): Observable<BookingDto> {
    return this.http.post<BookingDto>(this.bookingUrl, bookingDto);
  }

  // --- Helper Methods ---

  getServiceTypes(): Observable<ServiceType[]> {
    return this.http.get<ServiceType[]>('https://localhost:7213/api/ServiceTypes');
  }

  getLocations(): string[] {
    return this.locationList;
  }

  getCategories(): Category[] {
    return this.categories;
  }

  getSpecialtyTags(): SpecialtyTag[] {
    return this.specialtyTags;
  }

  // Get popular tags from backend
  getPopularTags(count: number = 10): Observable<string[]> {
    return this.http.get<string[]>(`https://localhost:7213/api/SpecialtyTags/popular?count=${count}`);
  }

  // Get available service cities
  getServiceCities(): Observable<string[]> {
    return this.http.get<string[]>('https://localhost:7213/api/ServiceAreas/cities');
  }

  // Get specialty categories with tags
  getCategoriesWithTags(): Observable<CategoryWithTags[]> {
    return this.http.get<CategoryWithTags[]>('https://localhost:7213/api/SpecialtyCategories');
  }

  getTagsByCategory(catId: number): string[] {
    return this.specialtyTags
      .filter((tag) => tag.catId === catId)
      .map((tag) => tag.name);
  }

  updateFilters(filters: Partial<SearchFilters>): void {
    const currentFilters = this.filtersSubject.value;
    this.filtersSubject.next({ ...currentFilters, ...filters });
  }

  getCurrentFilters(): SearchFilters {
    return this.filtersSubject.value;
  }

  resetFilters(): void {
    this.filtersSubject.next({
      serviceType: '',
      keyword: '',
      locations: [],
      tags: [],
      maxPrice: 10000,
      minRating: 0,
      sortOrder: 'default',
    });
  }
}
