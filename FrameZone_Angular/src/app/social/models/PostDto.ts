export interface PostDto {
  postId: number;
  userId: number;
  userName: string;
  avatar?: string | null;
  postContent: string;
  postType?: string | null;
  postTypeId?: number | null;
  updatedAt: string;
  isOwner?: boolean //就是本人
  likeCount?: number;
  isLiked?: boolean;
  shareCount?: number;
  isShared?: boolean;
  isSharedPost?: boolean;
  sharedPost?: PostDto | null;
  commentCount?: number;
}


