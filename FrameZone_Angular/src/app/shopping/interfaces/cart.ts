export interface CartItem {
  id: number;
  name: string;
  price: number;
  quantity: number;
  selected: boolean; // <-- 關鍵: 用來追蹤是否被勾選
  imageUrl?: string;
  specificationId?: number;
  sellerId: string | number;
  sellerName: string;
  sellerAvatar?: string;
}

export interface Coupon {
  id: number;
  name: string;
  discount: number; // 折扣金額
  code: string;
  expiryDate: Date;
  isSelected: boolean; // 是否被使用者選中
}
