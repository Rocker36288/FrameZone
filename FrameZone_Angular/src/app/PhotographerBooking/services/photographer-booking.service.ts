import { Injectable } from '@angular/core';
import { Category, Photographer, SearchFilters, SpecialtyTag } from '../models/photographer-booking.models';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PhotographerBookingService {
  // 模擬資料
  private readonly serviceTypes = [
    '人像寫真',
    '婚禮紀錄',
    '活動攝影',
    '商業廣告',
  ];
  private readonly locationList = [
    '台北',
    '新北',
    '桃園',
    '台中',
    '台南',
    '高雄',
    '基隆',
    '宜蘭',
    '新竹',
  ];

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

  private readonly photographers: Photographer[] = [
    {
      name: 'Mia Wedding',
      loc: '台北',
      type: '婚禮紀錄',
      tags: ['黑白風格', '微距拍攝'],
      price: 5000,
      rating: 4.8,
      reviews: 92,
      img: 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=400',
    },
    {
      name: 'Sunny Studio',
      loc: '台中',
      type: '商業廣告',
      tags: ['極簡主義'],
      price: 4200,
      rating: 5.0,
      reviews: 210,
      img: 'https://images.unsplash.com/photo-1554080353-a576cf803bda?w=400',
    },
    {
      name: 'Alex Photo',
      loc: '台北',
      type: '人像寫真',
      tags: ['日系清新', '底片感'],
      price: 3500,
      rating: 4.9,
      reviews: 156,
      img: 'https://images.unsplash.com/photo-1542038784456-1ea8e935640e?w=400',
    },
    {
      name: 'Emily Chen',
      loc: '新北',
      type: '人像寫真',
      tags: ['韓系簡約', '底片感'],
      price: 3800,
      rating: 4.7,
      reviews: 88,
      img: 'https://images.unsplash.com/photo-1600880292203-757bb62b4baf?w=400',
    },
    {
      name: 'David Lee',
      loc: '高雄',
      type: '活動攝影',
      tags: ['夜景專家', '空拍/航拍'],
      price: 6500,
      rating: 4.9,
      reviews: 134,
      img: 'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=400',
    },
    {
      name: 'Rachel Wang',
      loc: '台南',
      type: '婚禮紀錄',
      tags: ['美式寫實', '微距拍攝'],
      price: 5500,
      rating: 5.0,
      reviews: 167,
      img: 'https://images.unsplash.com/photo-1519741497674-611481863552?w=400',
    },
  ];

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

  constructor() {}

  getServiceTypes(): string[] {
    return this.serviceTypes;
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

  searchPhotographers(filters: SearchFilters): Photographer[] {
    let filtered = this.photographers.filter((p) => {
      const matchLoc =
        filters.locations.length === 0 || filters.locations.includes(p.loc);
      const matchSvc = !filters.serviceType || p.type === filters.serviceType;
      const matchPrice = p.price <= filters.maxPrice;
      const matchRating = p.rating >= filters.minRating;
      const matchKey =
        !filters.keyword ||
        p.name.toLowerCase().includes(filters.keyword.toLowerCase()) ||
        p.tags.some((t) =>
          t.toLowerCase().includes(filters.keyword.toLowerCase())
        );
      const matchTags =
        filters.tags.length === 0 ||
        filters.tags.every((t) => p.tags.includes(t));

      return (
        matchLoc &&
        matchSvc &&
        matchPrice &&
        matchRating &&
        matchKey &&
        matchTags
      );
    });

    // 排序
    switch (filters.sortOrder) {
      case 'priceAsc':
        filtered.sort((a, b) => a.price - b.price);
        break;
      case 'priceDesc':
        filtered.sort((a, b) => b.price - a.price);
        break;
      case 'ratingDesc':
        filtered.sort((a, b) => b.rating - a.rating);
        break;
    }

    // 限制最多顯示 12 位
    return filtered.slice(0, 12);
  }
}
