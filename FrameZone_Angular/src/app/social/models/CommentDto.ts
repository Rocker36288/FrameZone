export interface CommentDto {
  commentId: number;
  userId: number;
  displayName: string;
  avatar: string | null;
  commentTargetId: number;
  parentCommentId: number | null;
  commentContent: string;
  createdAt: string;
  updatedAt?: string;
  likeCount: number;
  isOwner: boolean;  //就是本人
  replies: CommentDto[]; // 支援無限層級的關鍵
}
