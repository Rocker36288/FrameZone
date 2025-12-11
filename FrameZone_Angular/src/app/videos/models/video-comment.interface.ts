export interface VideoCommentModel {
  id: number;
  userName: string;
  avatar: string;
  message: string;
  createdAt: Date;
  likes: number;
  replies?: VideoCommentModel[];
}
