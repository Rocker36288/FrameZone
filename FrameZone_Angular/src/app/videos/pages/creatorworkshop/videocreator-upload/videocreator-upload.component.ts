import { HttpClient, HttpEventType } from '@angular/common/http';
import { Component, ElementRef, ViewChild, OnDestroy } from '@angular/core';
import { NgIf, NgForOf } from '@angular/common';
import { interval, Subscription, switchMap, takeWhile, finalize, catchError, of } from 'rxjs';
import { VideoUploadService } from '../../../service/video-upload.service';
import { ProcessStatus, PrivacyStatus } from '../../../models/video.enum';
import { VideoPlayerComponent } from "../../../ui/video/video-player/video-player.component";
import { FormsModule } from '@angular/forms';
import { VideoPublishRequest } from '../../../models/videocreator-model';

interface VideoStatusResponse {
  videoUrl: string;
  processStatus: ProcessStatus;
  aiAuditResult: string | null;
  transcodeProgress: number;
}

@Component({
  selector: 'app-videocreator-upload',
  imports: [FormsModule, NgIf, NgForOf, VideoPlayerComponent],
  templateUrl: './videocreator-upload.component.html',
  styleUrl: './videocreator-upload.component.css'
})
export class VideocreatorUploadComponent implements OnDestroy {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  // ─── 檔案與影片資訊 ─────────────────
  selectedFile?: File;
  videoGuid = '';
  videoId?: number;
  videoplayuri = '';

  // ─── 狀態管理 ─────────────────────
  ProcessStatus = ProcessStatus;
  PrivacyStatus = PrivacyStatus;
  status: ProcessStatus = ProcessStatus.UPLOADING;
  reviewReason = '';
  transcodeProgress = 0;
  selectedPrivacy: PrivacyStatus = PrivacyStatus.PUBLIC;

  // ─── 上傳狀態 ─────────────────────
  uploading = false;
  uploadFinished = false;
  uploadProgress = 0;
  statusMessage = '';
  uploadFail = false;

  // ─── 縮圖管理 ─────────────────────
  thumbnails: string[] = [];
  selectedThumbnail = '';
  thumbnailsLoaded = false;
  thumbnailSaved = false;
  customThumbnailPreview?: string;

  // ─── 表單資料 ─────────────────────
  videoTitle = '';
  videoDescription = '';

  private pollSub?: Subscription;
  private readonly API_BASE = 'https://localhost:7213/api/VideoUpload';
  private readonly MAX_FILE_SIZE = 500 * 1024 * 1024; // 500MB
  private readonly POLL_INTERVAL = 3000; // 3 seconds

  constructor(
    private http: HttpClient,
    private videoUploadService: VideoUploadService
  ) { }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  // ═══════════════════════════════════
  // 檔案選擇與拖放
  // ═══════════════════════════════════

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.prepareUpload(file);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();

    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.prepareUpload(file);
    }
  }

  // ═══════════════════════════════════
  // 上傳流程
  // ═══════════════════════════════════

  prepareUpload(file: File): void {
    if (!this.validateFile(file)) {
      return;
    }

    this.selectedFile = file;
    this.resetUploadState();
    this.upload();
  }

  private validateFile(file: File): boolean {
    if (!file.type.startsWith('video/')) {
      this.showError('請選擇影片檔案');
      return false;
    }

    if (file.size > this.MAX_FILE_SIZE) {
      this.showError('檔案超過 500MB 限制');
      return false;
    }

    return true;
  }

  resetUploadState(): void {
    this.stopPolling();

    this.uploadFail = false;
    this.uploadFinished = false;
    this.uploading = false;
    this.uploadProgress = 0;
    this.statusMessage = '';
    this.thumbnails = [];
    this.selectedThumbnail = '';
    this.thumbnailsLoaded = false;
    this.thumbnailSaved = false;
    this.videoGuid = '';
    this.status = ProcessStatus.UPLOADING;
    this.transcodeProgress = 0;
    this.reviewReason = '';
    this.videoTitle = '';
    this.videoDescription = '';
    this.selectedPrivacy = PrivacyStatus.PUBLIC;
  }

  upload(): void {
    if (!this.selectedFile) return;

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.uploading = true;
    this.uploadProgress = 0;
    this.statusMessage = '上傳中...';
    this.status = ProcessStatus.UPLOADING;

    this.http.post<any>(`${this.API_BASE}/upload`, formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      finalize(() => {
        if (this.uploadProgress === 100 && !this.videoGuid) {
          this.uploading = false;
        }
      })
    ).subscribe({
      next: event => this.handleUploadEvent(event),
      error: err => this.handleUploadError(err)
    });
  }

  private handleUploadEvent(event: any): void {
    if (event.type === HttpEventType.UploadProgress && event.total) {
      this.uploadProgress = Math.round((event.loaded / event.total) * 100);
      this.statusMessage = `上傳中... ${this.uploadProgress}%`;
    }

    if (event.type === HttpEventType.Response) {
      this.uploading = false;
      this.handleUploadResponse(event.body);
    }
  }

  private handleUploadResponse(body: any): void {
    console.log('Upload response:', body);

    if (!body?.guid) {
      this.showError('無法取得影片 GUID');
      this.uploadFail = true;
      return;
    }

    this.videoGuid = body.guid;
    this.status = ProcessStatus.UPLOADED;

    if (body.reviewPassed === true) {
      this.uploadFinished = true;
      this.statusMessage = '影片審核通過,正在載入縮圖...';
      this.loadThumbnails();
      this.startPolling();
    } else {
      this.uploadFail = true;
      this.reviewReason = body.reviewReason || '未知原因';
      this.statusMessage = `影片審核未通過: ${this.reviewReason}`;
    }
  }

  private handleUploadError(err: any): void {
    console.error('上傳失敗:', err);
    this.uploading = false;
    this.uploadFail = true;
    this.statusMessage = err.error?.message || '上傳失敗,請稍後重試';
  }

  // ═══════════════════════════════════
  // 縮圖管理
  // ═══════════════════════════════════

  loadThumbnails(): void {
    if (!this.videoGuid) {
      console.error('無法載入縮圖: videoGuid 為空');
      return;
    }

    const body = { videoGuid: this.videoGuid };

    this.http.post<string[]>(`${this.API_BASE}/thumbnails-preview`, body)
      .subscribe({
        next: (res) => {
          console.log('縮圖載入成功:', res.length);
          this.thumbnails = res;
          this.thumbnailsLoaded = true;
          this.statusMessage = '請選擇影片縮圖';
        },
        error: (err) => {
          console.error('載入縮圖失敗:', err);
          this.statusMessage = '載入縮圖失敗,請重試';
        }
      });
  }

  selectThumbnail(thumb: string): void {
    this.selectedThumbnail = thumb;
  }

  saveThumbnail(): void {
    if (!this.selectedThumbnail) {
      this.statusMessage = '請先選擇一張縮圖';
      return;
    }

    const body = {
      videoGuid: this.videoGuid,
      thumbnailBase64: this.selectedThumbnail
    };

    this.http.post(`${this.API_BASE}/save-thumbnail`, body)
      .subscribe({
        next: () => {
          console.log('縮圖儲存成功');
          this.thumbnailSaved = true;
          this.statusMessage = '縮圖已儲存,請填寫影片資訊';
        },
        error: (err) => {
          console.error('儲存縮圖失敗:', err);
          this.statusMessage = '儲存縮圖失敗,請重試';
        }
      });
  }

  onThumbnailFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];

    // 檔案格式檢查
    if (!file.type.startsWith('image/')) {
      this.statusMessage = '請選擇圖片檔案';
      return;
    }

    // 檔案大小限制（例：2MB）
    if (file.size > 2 * 1024 * 1024) {
      this.statusMessage = '縮圖大小不可超過 2MB';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      this.customThumbnailPreview = reader.result as string;

      // 設為目前選擇的縮圖（沿用既有 saveThumbnail）
      this.selectedThumbnail = this.customThumbnailPreview;
      this.statusMessage = '已選擇自訂縮圖';
    };

    reader.readAsDataURL(file);
  }

  // ═══════════════════════════════════
  // 狀態輪詢
  // ═══════════════════════════════════

  startPolling(): void {
    this.stopPolling();

    if (!this.videoGuid) {
      console.error('無法開始輪詢: videoGuid 為空');
      return;
    }

    console.log('開始輪詢影片狀態, GUID:', this.videoGuid);

    this.pollSub = interval(this.POLL_INTERVAL)
      .pipe(
        switchMap(() => {
          console.log('輪詢中...');
          return this.videoUploadService.getVideoStatus(this.videoGuid).pipe(
            catchError(err => {
              console.error('輪詢請求失敗:', err);
              return of(null);
            })
          );
        }),
        takeWhile(res => {
          if (!res) return true; // 如果請求失敗,繼續輪詢

          const shouldContinue =
            res.processStatus !== ProcessStatus.READY &&
            res.processStatus !== ProcessStatus.FAILED_TRANSCODE &&
            res.processStatus !== ProcessStatus.FAILED_AUDIT;

          console.log('當前狀態:', res.processStatus, '是否繼續輪詢:', shouldContinue);
          return shouldContinue;
        }, true) // inclusive = true,最後一次也會發送
      )
      .subscribe({
        next: res => {
          if (res) {
            this.handlePollingResponse(res);
          }
        },
        error: err => {
          console.error('輪詢串流錯誤:', err);
        },
        complete: () => {
          console.log('輪詢完成');
          this.pollSub = undefined;
        }
      });
  }

  private stopPolling(): void {
    if (this.pollSub) {
      console.log('停止輪詢');
      this.pollSub.unsubscribe();
      this.pollSub = undefined;
    }
  }

  private handlePollingResponse(res: VideoStatusResponse): void {
    console.log('輪詢回應:', res);

    this.status = res.processStatus;
    this.reviewReason = res.aiAuditResult || '';
    this.transcodeProgress = res.transcodeProgress || 0;

    // 根據不同狀態更新 UI
    switch (res.processStatus) {
      case ProcessStatus.UPLOADED:
        this.statusMessage = '影片已上傳,等待處理...';
        break;

      case ProcessStatus.PRE_PROCESSING:
        this.statusMessage = '影片預處理中...';
        break;

      case ProcessStatus.TRANSCODING:
        this.statusMessage = `影片轉碼中 ${this.transcodeProgress}%`;
        break;

      case ProcessStatus.AI_AUDITING:
        this.statusMessage = 'AI 審核中...';
        break;

      case ProcessStatus.READY:
        this.statusMessage = '影片已準備完成!';
        this.videoplayuri = `https://localhost:7213/videos/${res.videoUrl}/hls/master.m3u8`;
        console.log('✅ 影片已準備完成, 播放 URI:', this.videoplayuri);
        break;

      case ProcessStatus.FAILED_TRANSCODE:
        this.statusMessage = '影片轉碼失敗';
        this.uploadFail = true;
        this.showError(`轉碼失敗: ${this.reviewReason}`);
        break;

      case ProcessStatus.FAILED_AUDIT:
        this.statusMessage = '影片審核失敗';
        this.uploadFail = true;
        this.showError(`審核失敗: ${this.reviewReason}`);
        break;

      default:
        this.statusMessage = '處理中...';
        console.warn('未知的狀態:', res.processStatus);
        break;
    }
  }

  // ═══════════════════════════════════
  // 表單提交
  // ═══════════════════════════════════

  onPrivacyChange(privacy: PrivacyStatus): void {
    this.selectedPrivacy = privacy;
  }

  publishVideo(): void {
    if (!this.videoTitle.trim()) {
      this.showError('請輸入影片標題');
      return;
    }

    if (this.status !== ProcessStatus.READY) {
      this.showError('影片尚未準備完成,請稍候');
      return;
    }

    const payload: VideoPublishRequest = {
      videoGuid: this.videoGuid!,
      title: this.videoTitle.trim(),
      description: this.videoDescription?.trim() || '',
      privacyStatus: this.selectedPrivacy
    };

    console.log('發佈影片 payload:', payload);

    this.http.post(`${this.API_BASE}/publish`, payload)
      .subscribe({
        next: () => {
          this.statusMessage = '影片已成功發佈';
          // TODO: 導向影片頁 or 後台
        },
        error: (err) => {
          console.error(err);
          this.showError('發佈失敗，請稍後再試');
        }
      });
  }


  saveDraft(): void {
    if (!this.videoTitle.trim()) {
      this.showError('請輸入影片標題');
      return;
    }

    const payload: VideoPublishRequest = {
      videoGuid: this.videoGuid!,
      title: this.videoTitle.trim(),
      description: this.videoDescription?.trim() || '',
      privacyStatus: PrivacyStatus.DRAFT
    };

    console.log('儲存草稿 payload:', payload);

    this.http.post(`${this.API_BASE}/draft`, payload)
      .subscribe({
        next: () => {
          this.statusMessage = '草稿已儲存';
        },
        error: () => {
          this.showError('草稿儲存失敗');
        }
      });
  }


  // ═══════════════════════════════════
  // 輔助方法
  // ═══════════════════════════════════

  private showError(message: string): void {
    alert(message);
  }

  navigateToEdit(): void {
    console.log('導航到編輯頁面');
  }

  // ═══════════════════════════════════
  // Getters for template
  // ═══════════════════════════════════

  get canShowThumbnails(): boolean {
    return this.uploadFinished && this.thumbnailsLoaded && !this.thumbnailSaved;
  }

  get canShowVideoForm(): boolean {
    return this.uploadFinished && this.thumbnailSaved;
  }

  get isTranscoding(): boolean {
    return this.status === ProcessStatus.TRANSCODING ||
      this.status === ProcessStatus.PRE_PROCESSING;
  }

  get isReady(): boolean {
    return this.status === ProcessStatus.READY;
  }

  get isProcessing(): boolean {
    return this.status === ProcessStatus.PRE_PROCESSING ||
      this.status === ProcessStatus.TRANSCODING ||
      this.status === ProcessStatus.AI_AUDITING;
  }

  get hasFailed(): boolean {
    return this.status === ProcessStatus.FAILED_TRANSCODE ||
      this.status === ProcessStatus.FAILED_AUDIT;
  }
}
