import { PrivacyStatus, ProcessStatus } from "./video.enum";

//影片上傳回傳
export interface VideoUploadResponse {
  success: boolean;
  message: String;
  videoId: number;
  guid: string;
  status: string;

}

export interface VideoPublishRequest {
  videoGuid: string;
  title: string;
  description?: string;
  privacyStatus: PrivacyStatus;
}

// 影片詳情資料（創作者工作室）
export class VideoDetailData {

  // ─── 識別 ─────────────────────
  id: number = 0;

  // ─── 核心內容 ─────────────────
  title: string = '';
  description: string = '';
  thumbnail: string = '';

  // ─── 時間相關 ─────────────────
  duration: number = 0; // 秒
  publishDate: Date = new Date();

  // ─── 成效數據 ─────────────────
  viewsCount: number = 0;
  likesCount: number = 0;
  commentCount: number = 0;

  // ─── 資源 / 技術 ──────────────
  createdDate: Date = new Date();
  updateDate: Date = new Date();
  resolution: string = '';

  videoUrl: string = '';
  processStatus: ProcessStatus = ProcessStatus.UPLOADING;
  privacyStatus: PrivacyStatus = PrivacyStatus.DRAFT;

  constructor(data?: Partial<VideoDetailData>) {
    if (data) {
      Object.assign(this, data);
    }
  }
}

// ===== 創作者分析總 DTO =====
export interface CreatorAnalyticsDto {
  totalViews: number;
  totalSubscribers: number;
  totalVideos: number;
  avgEngagement: number;
  viewsGrowth: number;
  subscribersGrowth: number;
  recentVideos: RecentVideoDto[];
  performanceChart: PerformanceChartDto[];
}

// ===== 近期影片 =====
export interface RecentVideoDto {
  videoId: number;
  title: string;
  views: number;
  likes: number;
  comments: number;
  publishDate: string;
  thumbnailUrl: string;
}

// ===== 圖表資料 =====
export interface PerformanceChartDto {
  date: string;
  views: number;
  engagement: number;
}

export interface VideoAIAuditResultDto {
  videoGuid: string;
  aiAuditResult: string;
}
