import { HttpClient, HttpEventType } from '@angular/common/http';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { VideoUploadResponse } from '../../../models/videocreator-model';
import { NgIf, NgForOf } from '@angular/common';
import { interval, Subscription, switchMap } from 'rxjs';
import { VideoUploadService } from '../../../service/video-upload.service';

@Component({
  selector: 'app-videocreator-upload',
  imports: [NgIf, NgForOf],
  templateUrl: './videocreator-upload.component.html',
  styleUrl: './videocreator-upload.component.css'
})
export class VideocreatorUploadComponent {

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  selectedFile!: File;
  videoGuid: string = '';
  status: string = '';
  reviewReason: string = '';
  transcodeProgress: number = 0;

  private pollSub?: Subscription;

  // ─── 狀態控制 ─────────────────────
  uploading = false;
  uploadFinished = false;

  uploadProgress = 0;
  statusMessage = '';
  uploadFail = false;

  // ─── 上傳後資料 ───────────────────
  videoId!: number;

  // ─── 圖片生成 ─────────────────────
  thumbnails: string[] = [];
  selectedThumbnail!: string;



  constructor(private http: HttpClient, private videoUploadService: VideoUploadService) { }

  // ─────────────────────────────
  // 檔案選擇
  // ─────────────────────────────
  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.prepareUpload(file);
  }

  // ─────────────────────────────
  // 拖放處理
  // ─────────────────────────────
  onDragOver(event: DragEvent) {
    event.preventDefault();
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
  }

  onDrop(event: DragEvent) {
    event.preventDefault();

    const file = event.dataTransfer?.files?.[0];
    if (!file) return;

    this.prepareUpload(file);
  }

  // ─────────────────────────────
  // 上傳準備
  // ─────────────────────────────
  prepareUpload(file: File) {
    // 檢查格式
    if (!file.type.startsWith('video/')) {
      alert('請選擇影片檔案');
      return;
    }

    // 檢查大小（500MB）
    if (file.size > 500 * 1024 * 1024) {
      alert('檔案超過 500MB');
      return;
    }

    this.selectedFile = file;
    this.upload();
  }

  // ─────────────────────────────
  // 上傳影片（第 7 步）
  // ─────────────────────────────
  upload() {
    if (!this.selectedFile) return;
    // 🔹 重置狀態，允許重新上傳
    this.uploadFail = false;
    this.uploadFinished = false;
    this.uploading = false;
    this.uploadProgress = 0;
    this.statusMessage = '';

    const formData = this.prepareFormData(this.selectedFile);

    this.startUpload(formData);
  }

  // ──────────────
  // 1️⃣ 準備 FormData
  // ──────────────
  private prepareFormData(file: File): FormData {
    const formData = new FormData();
    formData.append('file', file);
    return formData;
  }

  // ──────────────
  // 2️⃣ 開始上傳並監控進度
  // ──────────────
  private startUpload(formData: FormData) {
    this.uploading = true;
    this.uploadProgress = 0;
    this.statusMessage = '上傳中...';

    this.http.post<any>('https://localhost:7213/api/VideoUpload/upload', formData, {
      reportProgress: true,
      observe: 'events'
    }).subscribe({
      next: event => this.handleUploadEvent(event),
      error: () => this.handleUploadError()
    });
  }

  // ──────────────
  // 3 處理上傳事件
  // ──────────────
  private handleUploadEvent(event: any) {
    if (event.type === HttpEventType.UploadProgress && event.total) {
      this.uploadProgress = Math.round((event.loaded / event.total) * 100);
    }
    if (event.type === HttpEventType.Response) {
      const body = event.body;
      console.log(body);
      if (body.guid) {
        this.videoGuid = body.guid;
        this.handleReview(body)
      } else {
        this.statusMessage = '無法取得影片 GUID';
      }
    }
  }


  // ──────────────
  // 4️⃣ 處理審核結果
  // ──────────────
  private handleReview(body: any) {
    this.uploading = false; // 無論如何都結束 uploading

    if (body.reviewPassed === true) {
      this.uploadFinished = true;
      this.statusMessage = '影片審核通過';
      this.loadThumbnails(); // 只有通過才載入縮圖
    } else {
      this.uploadFail = true;
      this.uploadFinished = false;
      this.statusMessage = '影片審核未通過';
    }
  }
  // ──────────────
  // 5️⃣ 上傳錯誤處理
  // ──────────────
  private handleUploadError() {
    this.uploadFail = true;
    this.uploading = false;
    this.statusMessage = '上傳失敗';
  }

  loadThumbnails() {
    const body = { videoGuid: this.videoGuid };

    this.http.post<string[]>('https://localhost:7213/api/VideoUpload/thumbnails-preview', body)
      .subscribe({
        next: (res) => {
          this.thumbnails = res;
          this.statusMessage = '請選擇影片縮圖';
        },
        error: (err) => {
          console.error('載入縮圖失敗:', err);
          this.statusMessage = '載入縮圖失敗，請重試';
        }
      });
  }

  confirmThumbnail() {
    this.saveThumbnail();
    this.uploadFinished = true; // ⭐ 正式進入下一階段
  }

  selectThumbnail(thumb: string) {
    this.selectedThumbnail = thumb;
  }

  saveThumbnail() {
    if (!this.selectedThumbnail) {
      this.statusMessage = '請先選擇一張縮圖';
      return;
    }

    const body = {
      videoGuid: this.videoGuid,
      thumbnailBase64: this.selectedThumbnail
    };

    this.http.post('https://localhost:7213/api/VideoUpload/save-thumbnail', body)
      .subscribe({
        next: (res) => {
          console.log('縮圖儲存成功', res);
          this.statusMessage = '縮圖已儲存完成';
        },
        error: (err) => {
          console.error('儲存縮圖失敗:', err);
          this.statusMessage = '儲存縮圖失敗，請重試';
        }
      });
  }


  //影片轉碼
  ngOnInit(): void {
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
  }

  startPolling() {
    this.pollSub = interval(3000) // 每 3 秒輪詢一次
      .pipe(
        switchMap(() => this.videoUploadService.getVideoStatus(this.videoGuid))
      )
      .subscribe(res => {
        this.status = res.status;
        this.reviewReason = res.reviewReason;
        this.transcodeProgress = res.transcodeProgress || 0;

        if (res.status === 'Approved') {
          // 影片審核通過，跳轉到編輯或轉碼頁面
          this.navigateToEdit();
        } else if (res.status === 'Rejected') {
          // 顯示駁回訊息
          alert(`影片被拒絕：${this.reviewReason}`);
          this.pollSub?.unsubscribe();
        }
      });
  }

  navigateToEdit() {
    // 例如路由導向 /videos/edit/{videoGuid}
    // 或顯示同頁面編輯功能
  }
}
