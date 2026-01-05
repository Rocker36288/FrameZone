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

export interface ShopProduct {
  productId: number;
  name: string;
  image: string;
  description: string;
  price: number;
  seller: {
    name: string;
    avatar: string;
  };
  postedDate: string;
  sales: number;
  categoryId: number;
  sellerCategoryIds: number[];
  isFavorite: boolean;
  averageRating: number;
  reviewCount: number;
}

// 商品詳情
export interface ProductDetail {
  productId: number;
  userId: number;
  productName: string;
  description: string;
  categoryId: number;
  categoryName?: string;
  status: string;
  auditStatus: string;
  images: ProductImage[];
  specifications: ProductSpecification[];
  seller: {
    userId: number;
    displayName: string;
    avatar?: string;
    rating: number;
    reviewCount: number;
  };
  createdAt: string;
  updatedAt: string;
  isFavorite: boolean;
  averageRating: number;
  reviewCount: number;
  reviews: ProductReview[];
}

export interface ProductImage {
  productImageId: number;
  imageUrl: string;
  isMainImage: boolean;
  displayOrder: number;
}

export interface ProductSpecification {
  specificationId: number;
  specName: string;
  price: number;
  stock: number;
  sku?: string;
}

export interface ProductReview {
  reviewId: number;
  reviewerName: string;
  reviewerAvatar: string;
  rating: number;
  content: string;
  reply?: string;
  createdAt: string;
  imageUrls: string[];
}
