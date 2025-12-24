import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject, tap } from 'rxjs';
import { PostDto } from "../models/PostDto";



@Injectable({
  providedIn: 'root'
})
export class PostService {
  private apiUrl = 'https://localhost:7213/api/posts';

  // 1. 建立一個 Subject 訊號
  private _refreshNeeded$ = new Subject<void>();

  // 2. 暴露一個 Observable 供列表元件訂閱
  get refreshNeeded$() {
    return this._refreshNeeded$;
  }

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
    }).pipe(tap(() => {
      // 3. 發送「需要重新整理」的通知
      this._refreshNeeded$.next();
    }));
  }

  /**編輯貼文 */
  editPost(postId: number, postContent: string): Observable<PostDto> {
    return this.http.put<PostDto>(`${this.apiUrl}/${postId}`, { postContent: postContent })
  }

  /**刪除貼文 */
  deletePost(postId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${postId}`)
  }
}
