import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ShoppingUserProfile } from '../../interfaces/user-profile';

/**
 * Shopping 模組專用的使用者資料服務
 */
@Injectable({
    providedIn: 'root'
})
export class ShoppingUserService {
    private apiUrl = 'https://localhost:7213/api/shopping/member';

    constructor(private http: HttpClient) { }

    /**
     * 取得使用者資料（轉換為 Shopping 模組格式）
     */
    getUserProfile(): Observable<ShoppingUserProfile> {
        return this.http.get<any>(`${this.apiUrl}/profile`).pipe(
            map(response => {
                if (response.success && response.data) {
                    const profile = response.data;
                    return {
                        account: profile.account || '',
                        email: profile.email || '',
                        phone: profile.phone || '',
                        displayName: profile.displayName || '',
                        avatar: profile.avatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(profile.displayName || 'U')}&background=random`,
                        realName: profile.realName || '',
                        gender: profile.gender || '',
                        birthDate: profile.birthDate || ''
                    };
                }
                // 返回預設值
                return {
                    account: '',
                    email: '',
                    phone: '',
                    displayName: '',
                    avatar: 'https://ui-avatars.com/api/?name=U&background=random',
                    realName: '',
                    gender: '',
                    birthDate: ''
                };
            })
        );
    }

    /**
     * 更新使用者資料
     */
    updateUserProfile(profile: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/profile`, profile);
    }
}
