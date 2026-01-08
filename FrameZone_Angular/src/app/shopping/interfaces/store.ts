export interface PickupStore {
    convenienceStoreId: number;
    recipientName: string;
    phoneNumber: string;
    convenienceStoreCode: string;
    convenienceStoreName: string;
    isDefault: boolean;
    createdAt: string;
}

export interface CreatePickupStoreDto {
    recipientName: string;
    phoneNumber: string;
    convenienceStoreCode: string;
    convenienceStoreName: string;
    isDefault: boolean;
}

export interface StoreApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
}
