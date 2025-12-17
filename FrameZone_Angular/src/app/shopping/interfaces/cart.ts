export interface CartItem {
  id: number;
  name: string;
  price: number;
  quantity: number;
  selected: boolean; // <-- 關鍵: 用來追蹤是否被勾選
  // 您可以加入其他屬性，例如: quantity: number;
}

export interface Coupon {
  id: number;
  name: string;
  discount: number; // 折扣金額
  code: string;
  expiryDate: Date;
  isSelected: boolean; // 是否被使用者選中
}
