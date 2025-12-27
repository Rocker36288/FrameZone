export interface PostDto {
  postId: number;
  userId: number;
  userName: string;
  postContent: string;
  updatedAt: string;
  comments: CommentDto[]; //貼文的留言
}

export interface CommentDto {
  commentId: number;
  userId: number;
  userName: string;
  content: string;
  updatedAt: string;
  replies?: CommentDto[]; // 支援留言裡的回覆
}
