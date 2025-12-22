import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
// import { environment } from '../../environments/environment';
import { ChannelCard, ChannelHome, VideoCardData, VideoCommentCard, VideoCommentRequest } from '../models/video-model';

@Injectable({
  providedIn: 'root'
})
export class VideoService {
  // private apiBase = environment.apiBase; // e.g. 'https://localhost:7213/api'
  private apiBase = 'https://localhost:7213/api'


  constructor(private http: HttpClient) { }

  // ===== 取得影片資訊 =====
  getVideo(videoGuid: string): Observable<VideoCardData> {
    return this.http.get<VideoCardData>(`${this.apiBase}/videos/${videoGuid}`);
  }

  // ===== 取得影片推薦 =====
  getVideoRecommend(): Observable<VideoCardData[]> {
    return this.http.get<VideoCardData[]>(`${this.apiBase}/videos/Recommend`);
  }

  // ===== 取得影片留言 =====
  getVideoComments(videoGuid: string): Observable<VideoCommentCard[]> {
    return this.http.get<VideoCommentCard[]>(`${this.apiBase}/videos/${videoGuid}/comment`);
  }

  // ===== 取得頻道資訊 =====
  getChannelCard(id: number): Observable<ChannelCard> {
    return this.http.get<ChannelCard>(`${this.apiBase}/videos/channel/${id}`);
  }

  // ===== 取得頻道首頁資訊 =====
  getChannelHome(id: number): Observable<ChannelHome> {
    return this.http.get<ChannelHome>(`${this.apiBase}/videos/channel/home/${id}`);
  }

  // ===== 取得頻道首頁影片 =====
  getChannelVideos(id: number): Observable<VideoCardData[]> {
    return this.http.get<VideoCardData[]>(`${this.apiBase}/videos/channel/${id}/videos`);
  }

  // ===== 發表影片留言 =====
  postVideoComment(req: VideoCommentRequest): Observable<VideoCommentCard> {
    return this.http.post<VideoCommentCard>(`${this.apiBase}/videos/comment/publish`, req);
  }

  // ===== 取得影片上傳狀態 =====
  getVideoStatus(videoGuid: string): Observable<any> {
    return this.http.get(`${this.apiBase}/videoupload/${videoGuid}/status`)
  }
}
