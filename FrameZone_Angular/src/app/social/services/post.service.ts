import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface PostDto {
  postId: number;
  userId: number;
  content: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class PostService {
  private apiUrl = 'https://localhost:7213/api/posts';

  constructor(private http: HttpClient) { }

  /**取得多筆貼文 */
  getPosts(): Observable<PostDto[]> {
    return this.http.get<PostDto[]>(`${this.apiUrl}`)
  }
  /**取得貼文 */
  getPost(postId: number): Observable<PostDto> {
    return this.http.get<PostDto>(`${this.apiUrl}/${postId}`)
  }

  /**新增貼文 */
  createPost(postContent: string): Observable<any> {
    return this.http.post(this.apiUrl, {
      postContent: postContent
    });
  }

  /**編輯貼文 */
  editPost(postId: number, content: string): Observable<PostDto> {
    return this.http.put<PostDto>(`${this.apiUrl}/${postId}`, { content: content })
  }

  /**刪除貼文 */
  deletePost(postId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${postId}`)
  }
}
