/**
 * 收件地址相關的介面定義
 */

/**
 * 收件地址資料
 */
export interface ReceivingAddress {
  addressId: number;
  recipientName: string;
  phoneNumber: string;
  fullAddress: string;
  isDefault: boolean;
  createdAt: Date;
}

/**
 * 建立新收件地址的 DTO
 */
export interface CreateAddressDto {
  recipientName: string;
  phoneNumber: string;
  fullAddress: string;
  isDefault?: boolean;
}

/**
 * API 回應格式
 */
export interface AddressApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
  errors?: string[];
}
