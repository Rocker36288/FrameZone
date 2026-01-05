import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CreatorAnalyticsDto, VideoAIAuditResultDto, VideoDetailData } from '../models/videocreator-model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class VideoCreatorService {

  // private apiBase = environment.apiBase; // e.g. 'https://localhost:7213/api'
  private apiBase = 'https://localhost:7213/api'


  constructor(private http: HttpClient) { }

  // ===== 取得影片創作者近期影片 =====
  getRecentUploadVideos(count: number = 5): Observable<VideoDetailData[]> {
    return this.http.get<VideoDetailData[]>(
      `${this.apiBase}/VideoCreator/RecentUpload`,
      { params: { count } }
    );
  }

  // ===== 取得單一影片詳細編輯資料 =====
  getVideoForEdit(guid: string): Observable<VideoDetailData> {
    return this.http.get<VideoDetailData>(
      `${this.apiBase}/VideoCreator/edit/${guid}`
    );
  }

  // ===== 單一影片編輯資料 =====
  updateVideo(
    guid: string,
    payload: {
      title: string;
      description: string;
      privacyStatus: string;
    }
  ): Observable<void> {
    return this.http.patch<void>(
      `${this.apiBase}/VideoCreator/edit/${guid}/update`,
      payload
    );
  }

  uploadThumbnail(guid: string, file: File): Observable<VideoDetailData> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<VideoDetailData>(`${this.apiBase}/VideoCreator/edit/${guid}/thumbnail`, formData);
  }

  // ===============================
  // 創作者數據分析
  // ===============================
  getCreatorAnalytics(
    period: '7days' | '30days' | '90days' = '7days'
  ): Observable<CreatorAnalyticsDto> {

    const params = new HttpParams()
      .set('period', period);

    return this.http.get<CreatorAnalyticsDto>(
      `${this.apiBase}/VideoCreator/analytics`,
      { params }
    );
  }
  // ===============================
  // 取得影片 AI 審核結果
  // ===============================
  getVideoAIAuditResult(guid: string): Observable<VideoAIAuditResultDto> {
    return this.http.get<VideoAIAuditResultDto>(
      `${this.apiBase}/VideoCreator/${guid}/ai-result`
    );
  }
}
