import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class CartApiService {
    private http = inject(HttpClient);
    private readonly apiUrl = 'https://localhost:7213/api/shopping/cart';

    getCart(): Observable<any[]> {
        return this.http.get<any[]>(this.apiUrl);
    }

    addToCart(specificationId: number, quantity: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/add`, { specificationId, quantity });
    }

    updateCartItem(specificationId: number, quantity: number): Observable<any> {
        return this.http.put(`${this.apiUrl}/update`, { specificationId, quantity });
    }

    removeFromCart(specificationId: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/remove/${specificationId}`);
    }

    clearCart(): Observable<any> {
        return this.http.delete(`${this.apiUrl}/clear`);
    }

    syncCart(cartItems: { specificationId: number, quantity: number }[]): Observable<any> {
        return this.http.post(`${this.apiUrl}/sync`, cartItems);
    }
}
