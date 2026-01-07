export interface MessageDto {
  messageId: number;
  senderUserId: number;
  messageContent: string;
  messageType: string;
  stickerId?: number;
  mediaUrl?: string;
  thumbnailUrl?: string;
  productId?: number;
  orderId?: number;
  linkTitle?: string;
  linkDescription?: string;
  createdAt: string;
  isOwner?: boolean;
}

export interface SendShopMessageDto {
  messageType: string; // text, image, sticker, product, order, link, video
  messageContent?: string;
  productId?: number;
  orderId?: number;
  stickerId?: number;
  mediaUrl?: string;
  thumbnailUrl?: string;
  linkTitle?: string;
  linkDescription?: string;
}
