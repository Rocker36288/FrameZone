import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Faq } from '../../interfaces/faq';

/**
 * FAQ API 回應介面
 */
export interface FaqApiResponse {
    success: boolean;
    data: Faq[];
    message: string;
}

/**
 * FAQ 服務
 * 提供常見問題的查詢功能
 */
@Injectable({
    providedIn: 'root'
})
export class FaqService {
    private apiUrl = 'https://localhost:7213/api/shopping/faq';

    constructor(private http: HttpClient) { }

    /**
     * 取得所有 FAQ
     * @returns FAQ 列表
     */
    getFaqs(): Observable<FaqApiResponse> {
        return this.http.get<FaqApiResponse>(this.apiUrl);
    }

    /**
     * 依分類取得 FAQ
     * @param category 分類名稱
     * @returns FAQ 列表
     */
    getFaqsByCategory(category: string): Observable<FaqApiResponse> {
        return this.http.get<FaqApiResponse>(`${this.apiUrl}/${category}`);
    }
}
