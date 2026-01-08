import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PickupStore, CreatePickupStoreDto, StoreApiResponse } from '../../interfaces/store';

@Injectable({
    providedIn: 'root'
})
export class StoreService {
    private apiUrl = 'https://localhost:7213/api/shopping/stores';

    constructor(private http: HttpClient) { }

    getUserStores(): Observable<StoreApiResponse<PickupStore[]>> {
        return this.http.get<StoreApiResponse<PickupStore[]>>(this.apiUrl);
    }

    createStore(dto: CreatePickupStoreDto): Observable<StoreApiResponse<PickupStore>> {
        return this.http.post<StoreApiResponse<PickupStore>>(this.apiUrl, dto);
    }

    updateStore(storeId: number, dto: CreatePickupStoreDto): Observable<StoreApiResponse<any>> {
        return this.http.put<StoreApiResponse<any>>(`${this.apiUrl}/${storeId}`, dto);
    }

    deleteStore(storeId: number): Observable<StoreApiResponse<any>> {
        return this.http.delete<StoreApiResponse<any>>(`${this.apiUrl}/${storeId}`);
    }

    setDefaultStore(storeId: number): Observable<StoreApiResponse<any>> {
        return this.http.put<StoreApiResponse<any>>(`${this.apiUrl}/${storeId}/default`, {});
    }
}
