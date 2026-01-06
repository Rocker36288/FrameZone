import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface FavoriteItem {
    favoriteId: number;
    productId: number;
    name: string;
    price: number;
    imageUrl: string;
    date: string;
    sellerId: number;
    sellerName: string;
}

@Injectable({
    providedIn: 'root'
})
export class FavoriteService {
    private apiUrl = 'https://localhost:7213/api/Favorites';

    constructor(private http: HttpClient) { }

    /**
     * 取得目前登入使用者的收藏清單
     */
    getUserFavorites(): Observable<FavoriteItem[]> {
        return this.http.get<FavoriteItem[]>(this.apiUrl);
    }

    /**
     * 切換收藏狀態 (後續可擴充實作)
     */
    toggleFavorite(productId: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${productId}`, {});
    }
}
