export interface Review {
    reviewId: number;
    reviewerName: string;
    reviewerAvatar: string;
    rating: number;
    content: string;
    reply?: string;
    createdAt: string;
    imageUrls: string[];
    reviewType: string;
    targetName: string;
    targetImageUrl: string;
}

export interface CreateReviewPayload {
    orderId: number;
    productId: number;
    rating: number;
    content: string;
    // File objects are handled separately in FormData construction
    images?: File[];
}
