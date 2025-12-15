import { PrivacyStatus, ProcessStatus } from "./video.enum";

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
  uploadDate: Date = new Date();

  // ─── 成效數據 ─────────────────
  viewsCount: number = 0;
  likesCount: number = 0;
  commentCount: number = 0;

  // ─── 資源 / 技術 ──────────────
  videoUrl: string = '';
  processStatus: ProcessStatus = ProcessStatus.UPLOADING;
  privacyStatus: PrivacyStatus = PrivacyStatus.DRAFT;

  constructor(data?: Partial<VideoDetailData>) {
    if (data) {
      Object.assign(this, data);
    }
  }
}
