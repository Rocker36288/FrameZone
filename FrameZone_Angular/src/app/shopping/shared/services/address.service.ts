import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReceivingAddress, CreateAddressDto, AddressApiResponse } from '../../interfaces/address';

/**
 * 收件地址服務
 * 提供收件地址的 CRUD 操作
 */
@Injectable({
    providedIn: 'root'
})
export class AddressService {
    private apiUrl = 'https://localhost:7213/api/shopping/addresses';

    constructor(private http: HttpClient) { }

    /**
     * 取得使用者的所有收件地址
     * @returns 收件地址列表
     */
    getUserAddresses(): Observable<AddressApiResponse<ReceivingAddress[]>> {
        return this.http.get<AddressApiResponse<ReceivingAddress[]>>(this.apiUrl);
    }

    /**
     * 建立新的收件地址
     * @param dto 建立地址 DTO
     * @returns 建立結果
     */
    createAddress(dto: CreateAddressDto): Observable<AddressApiResponse<ReceivingAddress>> {
        return this.http.post<AddressApiResponse<ReceivingAddress>>(this.apiUrl, dto);
    }
}
