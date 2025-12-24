// src/app/shopping/interfaces/product.interface.ts

export interface Product {
  id: string;
  name: string;
  image: string;
  sku: string;
  category: string;
  price: number;
  stock: number;
  status: 'active' | 'inactive' | 'sold_out' | 'violation';
  sales: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface ProductStats {
  total: number;
  active: number;
  inactive: number;
  soldOut: number;
  violation: number;
  shipping: number;
}

export type ProductStatus = 'all' | 'active' | 'inactive' | 'sold_out' | 'violation' | 'shipping';

export type SortOrder = 'newest' | 'oldest' | 'price_high' | 'price_low' | 'sales_high' | 'sales_low';
