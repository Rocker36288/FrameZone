import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductApiService {

  constructor() { }
  // 1. 使用 inject 取得 HttpClient
  private http = inject(HttpClient);

  // 2. 定義後端 API 位址
  private readonly apiUrl = 'https://localhost:7213/api/products';

  // 3. 取得所有商品的請求方法
  getProducts(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  // 取得商品詳情
  getProductDetail(productId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${productId}`);
  }

  // 取得類似商品
  getSimilarProducts(productId: number): Observable<any[]> {
    // 假設 API 路徑為 /products/{id}/similar
    return this.http.get<any[]>(`${this.apiUrl}/${productId}/similar`);
  }

  getRecommendedProducts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/recommended`);
  }

  getPopularProducts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/popular`);
  }

  getProductsByIds(ids: number[]): Observable<any[]> {
    return this.http.post<any[]>(`${this.apiUrl}/batch`, ids);
  }

  // 取得特定賣家的商品
  getProductsBySeller(sellerId: string | number): Observable<any[]> {
    return this.http.get<any[]>(`https://localhost:7213/api/sellers/${sellerId}/products`);
  }

  // 取得賣家資料
  getSellerProfile(sellerId: string | number): Observable<any> {
    return this.http.get<any>(`https://localhost:7213/api/sellers/${sellerId}/profile`);
  }

  // 取得賣家自定義分類
  getSellerCategories(sellerId: string | number): Observable<any[]> {
    return this.http.get<any[]>(`https://localhost:7213/api/sellers/${sellerId}/categories`);
  }

  // 取得賣家評價 (收到的)
  getSellerReviews(sellerId: string | number, take: number = 20, skip: number = 0): Observable<any[]> {
    return this.http.get<any[]>(`https://localhost:7213/api/Reviews/seller/${sellerId}?take=${take}&skip=${skip}`);
  }

  // 取得使用者發出的評價
  getUserSentReviews(userId: string | number, take: number = 20, skip: number = 0): Observable<any[]> {
    return this.http.get<any[]>(`https://localhost:7213/api/Reviews/user/${userId}?take=${take}&skip=${skip}`);
  }
}
