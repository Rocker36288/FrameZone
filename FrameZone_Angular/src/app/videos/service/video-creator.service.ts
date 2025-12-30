import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { VideoDetailData } from '../models/videocreator-model';
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
}
