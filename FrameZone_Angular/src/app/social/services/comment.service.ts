import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CommentDto } from '../models/CommentDto'; // 請根據你的路徑調整

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  // 使用 Angular 19 推薦的 inject 語法
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7213/api/Comments';

  // 1. 取得貼文的所有留言 (樹狀結構)
  getCommentsByPost(postId: number): Observable<CommentDto[]> {
    return this.http.get<CommentDto[]>(`${this.apiUrl}/post/${postId}`);
  }

  // 2. 新增留言 (包含主留言與回覆)
  createComment(dto: { postId: number; parentCommentId: number | null; commentContent: string }): Observable<CommentDto> {
    return this.http.post<CommentDto>(this.apiUrl, dto);
  }

  // 3. 刪除留言
  deleteComment(commentId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${commentId}`);
  }

  // 4. 修改留言
  updateComment(commentId: number, content: string): Observable<CommentDto> {
    return this.http.put<CommentDto>(`${this.apiUrl}/${commentId}`, { commentContent: content });
  }
}
