import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PostService {
  private apiUrl = 'https://localhost:7213/api/posts';

  constructor(private http: HttpClient) { }

  createPost(postContent: string): Observable<any> {
    return this.http.post(this.apiUrl, {
      postContent: postContent
    });
  }
}
