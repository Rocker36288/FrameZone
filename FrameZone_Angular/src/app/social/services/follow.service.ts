import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FollowUser } from '../models/follow.models';

@Injectable({
  providedIn: 'root'
})
export class FollowService {
  private apiUrl = 'https://localhost:7213/api/follows';

  constructor(private http: HttpClient) { }

  addFriend(userId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${userId}`, {});
  }

  getFollowing(userId: number): Observable<FollowUser[]> {
    return this.http.get<FollowUser[]>(`${this.apiUrl}/${userId}/following`);
  }

  getFollowers(userId: number): Observable<FollowUser[]> {
    return this.http.get<FollowUser[]>(`${this.apiUrl}/${userId}/followers`);
  }
}
