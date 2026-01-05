import { Component, HostListener } from '@angular/core';
import { NgClass, NgIf, NgForOf } from "@angular/common";
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { VideoCreatorService } from '../../../service/video-creator.service';
import { VideoDetailData } from '../../../models/videocreator-model';
import { VideoPlayerComponent } from "../../../ui/video/video-player/video-player.component";
import { PrivacyStatus } from '../../../models/video.enum';
import { ViewChild, ElementRef } from '@angular/core';
import { VideosReviewModalComponent } from "../../../ui/videos-review-modal/videos-review-modal.component";


@Component({
  selector: 'app-videocreator-editvideo',
  imports: [DatePipe, NgClass, NgIf, NgForOf, FormsModule, VideoPlayerComponent, VideosReviewModalComponent],
  templateUrl: './videocreator-editvideo.component.html',
  styleUrl: './videocreator-editvideo.component.css'
})
export class VideocreatorEditvideoComponent {
  PrivacyStatus = PrivacyStatus;

  video?: VideoDetailData;
  guid!: string;
  isLoading = true; // 新增載入狀態
  loadError = false; // 新增錯誤狀態

  hasUnsavedChanges = false;

  videoUrl: string = ''

  // ===== Inline Edit 狀態 =====
  editingTitle = false;
  editingDescription = false;
  editingPrivacy = false;

  editTitle = '';
  editDescription = '';
  editPrivacy!: PrivacyStatus;

  //=======縮圖
  thumbnailFile?: File;
  thumbnailPreview?: string;
  @ViewChild('thumbnailInput') thumbnailInput!: ElementRef<HTMLInputElement>;



  constructor(
    private route: ActivatedRoute,
    private videoService: VideoCreatorService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.guid = this.route.snapshot.paramMap.get('guid')!;
    this.loadVideo();

  }

  loadVideo() {
    this.isLoading = true;
    this.loadError = false;

    this.videoService.getVideoForEdit(this.guid).subscribe({
      next: (data) => {
        this.video = data;
        this.isLoading = false;
        this.videoUrl = 'https://localhost:7213/api/videoplayer/' + data.videoUrl
        this.video!.thumbnail =
          'https://localhost:7213/api/Videos/video-thumbnail/' + this.video!.videoUrl;
        console.log(data)
        console.log(this.video)
      },
      error: (err) => {
        this.isLoading = false;
        this.loadError = true;

        if (err.status === 403 || err.status === 404) {
          alert('你無權編輯此影片');
          this.router.navigate(['/']); // 導回首頁或影片列表
        }
      }
    });
  }

  // ====== 狀態樣式 ======
  getProcessStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'completed': return 'bg-success-lt';
      case 'processing': return 'bg-warning-lt';
      case 'failed': return 'bg-danger-lt';
      default: return 'bg-secondary-lt';
    }
  }

  getPrivacyStatusClass(status: string): string {
    switch (status?.toUpperCase()) {
      case 'PUBLIC': return 'bg-success-lt';
      case 'UNLISTED': return 'bg-warning-lt';
      case 'PRIVATE': return 'bg-danger-lt';
      case 'DRAFT': return 'bg-secondary-lt';
      default: return 'bg-secondary-lt';
    }
  }

  // ====== 工具 ======
  getReadableSize(size: number): string {
    if (!size) return '0 B';
    if (size >= 1024 ** 3) return (size / 1024 ** 3).toFixed(2) + ' GB';
    if (size >= 1024 ** 2) return (size / 1024 ** 2).toFixed(2) + ' MB';
    if (size >= 1024) return (size / 1024).toFixed(2) + ' KB';
    return size + ' B';
  }

  getDuration(seconds: number): string {
    if (!seconds) return '00:00';
    const m = Math.floor(seconds / 60).toString().padStart(2, '0');
    const s = Math.floor(seconds % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  }

  deleteVideo() {
    if (!this.video) return;

    if (confirm(`確定要刪除「${this.video.title}」嗎？`)) {
      // TODO: 實作刪除 API
      alert('刪除影片（示意）');
    }
  }

  editThumbnail() {
    this.thumbnailInput.nativeElement.click();
  }

  onThumbnailSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];

    if (!file.type.startsWith('image/')) {
      alert('請選擇圖片檔案');
      return;
    }

    this.thumbnailFile = file;
    this.hasUnsavedChanges = true;

    // 即時預覽
    const reader = new FileReader();
    reader.onload = () => {
      this.thumbnailPreview = reader.result as string;
    };
    reader.readAsDataURL(file);
  }

  saveThumbnail() {
    if (!this.video || !this.thumbnailFile) return;

    this.videoService.uploadThumbnail(this.video.videoUrl, this.thumbnailFile).subscribe({
      next: () => {
        // 上傳成功後，把 preview 清空，更新 video.thumbnail 為最新
        this.video!.thumbnail = this.thumbnailPreview!;
        this.thumbnailPreview = undefined;
        this.thumbnailFile = undefined;
        this.hasUnsavedChanges = false;
        alert('縮圖已更新');
      },
      error: (err) => {
        console.error(err);
        alert('縮圖上傳失敗，請稍後再試');
      }
    });
  }


  // ===== Title =====
  startEditTitle() {
    if (!this.video) return;
    this.hasUnsavedChanges = true;
    this.editTitle = this.video.title;
    this.editingTitle = true;
  }

  saveTitle() {
    if (!this.video) return;

    if (this.editTitle.trim() === '') {
      alert('標題不能為空');
      return;
    }

    if (this.editTitle !== this.video.title) {
      this.video.title = this.editTitle;
    }
    this.editingTitle = false;
  }

  cancelEditTitle() {
    this.editingTitle = false;
  }

  // ===== Description =====
  startEditDescription() {
    if (!this.video) return;
    this.hasUnsavedChanges = true;
    this.editDescription = this.video.description;
    this.editingDescription = true;
  }

  cancelEditDescription() {
    this.editingDescription = false;
  }

  saveDescription() {
    if (!this.video) return;

    if (this.editDescription !== this.video.description) {
      this.video.description = this.editDescription;
    }
    this.editingDescription = false;
  }

  // ===== Privacy =====
  startEditPrivacy() {
    if (!this.video || !this.canEditPrivacy()) return;
    this.hasUnsavedChanges = true;
    this.editPrivacy = this.video.privacyStatus;
    this.editingPrivacy = true;
  }

  savePrivacy() {
    if (!this.video) return;

    if (this.editPrivacy !== this.video.privacyStatus) {
      this.video.privacyStatus = this.editPrivacy; // ✅ 不再報錯
      this.hasUnsavedChanges = true;
    }
    this.editingPrivacy = false;
  }

  cancelEditPrivacy() {
    this.editingPrivacy = false;
  }

  canEditPrivacy(): boolean {
    // return this.video?.processStatus === 'Completed';
    return true;
  }

  // ================監聽改變
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    if (this.hasUnsavedChanges) {
      $event.returnValue = true;
    }
  }

  // ================影片儲存
  saveAllChanges() {
    if (!this.video) return;

    // 先更新 metadata
    const payload = {
      title: this.editTitle || this.video.title,
      description: this.editDescription || this.video.description,
      privacyStatus: this.editPrivacy || this.video.privacyStatus
    };

    this.videoService.updateVideo(this.video.videoUrl, payload).subscribe({
      next: () => {

        // 如果有新縮圖，才呼叫縮圖上傳
        if (this.thumbnailFile) {
          this.videoService.uploadThumbnail(this.video!.videoUrl, this.thumbnailFile).subscribe({
            next: () => {
              // 縮圖更新成功後清除暫存
              // this.thumbnailPreview = undefined;
              this.thumbnailFile = undefined;

              // 完成所有操作
              this.hasUnsavedChanges = false;
              this.editingTitle = false;
              this.editingDescription = false;
              this.editingPrivacy = false;

              alert('影片及縮圖已更新');
            },
            error: (err) => {
              console.error(err);
              alert('影片已更新，但縮圖上傳失敗');
            }
          });
        } else {
          // 沒有縮圖變動
          this.hasUnsavedChanges = false;
          this.editingTitle = false;
          this.editingDescription = false;
          this.editingPrivacy = false;
          alert('影片已更新');
        }

      },
      error: (err) => {
        console.error(err);
        alert('影片更新失敗，請稍後再試');
      }
    });
  }
  // 父元件 TS
  // videoReviewResult = {
  //   "passed": true,
  //   "reason": "Content approved",
  //   "reviewedAt": "2026-01-03T09:29:40.408415Z",
  //   "sightengine": {
  //     "status": "success",
  //     "nudity": {
  //       "sexual_activity": 0.001,
  //       "sexual_display": 0.001,
  //       "erotica": 0.001,
  //       "very_suggestive": 0.001,
  //       "suggestive": 0.001,
  //       "mildly_suggestive": 0.001,
  //       "context": { "sea_lake_pool": 0.001, "outdoor_other": 0.99, "indoor_other": 0.001 }
  //     },
  //     "media": {
  //       "uri": "thumbnail_0_compressed.jpg"
  //     }
  //   }
  // };


  // 開啟 modal 方法
  showModal = false;
  videoReviewResult: string = "";
  isLoadingAIResult = false;

  openReviewModal() {
    this.isLoadingAIResult = true;
    this.videoService.getVideoAIAuditResult(this.video!.videoUrl)
      .subscribe({
        next: res => {
          this.videoReviewResult = res.aiAuditResult;
          console.log(this.videoReviewResult)
          this.isLoadingAIResult = false;
          this.showModal = true;
        },
        error: err => {
          console.error('取得 AI 審核結果失敗', err);
          this.videoReviewResult = 'ERROR';
          this.isLoadingAIResult = false;
        }
      });
  }

  closeReviewModal() {
    this.showModal = false;
    // this.videoReviewResult = undefined;
  }
}
