import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
// import { environment } from '../../environments/environment';
import { ChannelCard, ChannelHome, ChannelSpotlightDto, UpdateWatchHistoryRequest, VideoCardData, VideoCommentCard, VideoCommentRequest, VideoLikesDto, VideoLikesRequest, VideoSearchParams, VideoWatchHistoryDto } from '../models/video-model';

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

  // ===== 取得影片喜歡狀態 =====
  getVideoLikes(videoGuid: string): Observable<VideoLikesDto> {
    return this.http.get<VideoLikesDto>(`${this.apiBase}/videos/${videoGuid}/likecheck`);
  }

  //======影片按讚切換==========
  ToggleVideoLikes(videoGuid: string, req: VideoLikesRequest): Observable<VideoLikesDto> {
    return this.http.post<VideoLikesDto>(`${this.apiBase}/videos/${videoGuid}/liketoggle`, req);
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

  // 取得頻道追隨狀態
  checkChannelFollow(channelId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiBase}/videos/channels/${channelId}/followcheck`);
  }

  // 切換頻道追隨狀態
  toggleChannelFollow(channelId: number): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiBase}/videos/channels/${channelId}/followtoggle`, {});
  }

  // 更新觀看紀錄
  updateWatchHistory(videoId: number, lastPosition: number): Observable<boolean> {
    const body: UpdateWatchHistoryRequest = { videoId, lastPosition };
    return this.http.post<boolean>(`${this.apiBase}/videos/views/update`, body);
  }

  // 更新觀看紀錄
  GetWatchHistory(): Observable<VideoWatchHistoryDto[]> {
    return this.http.get<VideoWatchHistoryDto[]>(`${this.apiBase}/videos/views/history`);
  }

  // ===== 搜尋影片 =====
  searchVideos(params: VideoSearchParams): Observable<VideoCardData[]> {
    const queryParams: any = {};

    if (params.keyword) {
      queryParams.keyword = params.keyword;
    }

    queryParams.sortBy = params.sortBy ?? 'date';
    queryParams.sortOrder = params.sortOrder ?? 'desc';
    queryParams.take = params.take ?? 10;

    return this.http.get<VideoCardData[]>(
      `${this.apiBase}/videos/search`,
      { params: queryParams }
    );
  }

  // ===== 取得頻道 Spotlight（頻道卡片 + 最新影片）=====
  getChannelSpotlight(channelId: number): Observable<ChannelSpotlightDto> {
    return this.http.get<ChannelSpotlightDto>(
      `${this.apiBase}/videos/SpotlightVideos`,
      {
        params: { channelId }
      }
    );
  }
}
