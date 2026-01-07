export interface PostDto {
  postId: number;
  userId: number;
  userName: string;
  avatar?: string | null;
  postContent: string;
  updatedAt: string;
  isOwner?: boolean //就是本人
  likeCount?: number;
  isLiked?: boolean;
}


