import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateReviewPayload } from '../../interfaces/review';

@Injectable({
    providedIn: 'root'
})
export class ReviewService {
    private apiUrl = 'https://localhost:7213/api/reviews';

    constructor(private http: HttpClient) { }

    createReviews(reviews: CreateReviewPayload[]): Observable<any> {
        const formData = new FormData();

        reviews.forEach((review, index) => {
            formData.append(`[${index}].OrderId`, review.orderId.toString());
            formData.append(`[${index}].ProductId`, review.productId.toString());
            formData.append(`[${index}].Rating`, review.rating.toString());
            formData.append(`[${index}].Content`, review.content);

            if (review.images && review.images.length > 0) {
                review.images.forEach((file) => {
                    // 注意：後端 DTO 結構為 List<CreateReviewDto>，每個 DTO 有 List<IFormFile> Images
                    // 對應的 key 應該是 [index].Images
                    formData.append(`[${index}].Images`, file);
                });
            }
        });

        return this.http.post(`${this.apiUrl}/batch`, formData);
    }
}
