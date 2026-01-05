export interface OrderDto {
  orderItems: OrderItem[];
  totalAmount: number;
  paymentMethod: string;
  returnURL: string;
  optionParams: {};
}

export interface OrderItem {
  id: number;
  name: string;
  price: number;
  quantity: number;
}


