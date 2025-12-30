import { Component, HostListener } from '@angular/core';
import { NgClass, NgIf, NgForOf } from "@angular/common";
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { VideoCreatorService } from '../../../service/video-creator.service';
import { VideoDetailData } from '../../../models/videocreator-model';
import { VideoPlayerComponent } from "../../../ui/video/video-player/video-player.component";

@Component({
  selector: 'app-videocreator-editvideo',
  imports: [DatePipe, NgClass, NgIf, NgForOf, FormsModule, VideoPlayerComponent],
  templateUrl: './videocreator-editvideo.component.html',
  styleUrl: './videocreator-editvideo.component.css'
})
export class VideocreatorEditvideoComponent {

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
  editPrivacy = '';

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
    switch (status?.toLowerCase()) {
      case 'public': return 'bg-success-lt';
      case 'private': return 'bg-danger-lt';
      case 'unlisted': return 'bg-warning-lt';
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
    // TODO: 實作縮圖編輯功能
    alert('編輯縮圖功能開發中');
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
      // TODO: PATCH /videos/{id}/metadata
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
      // TODO: PATCH /videos/{id}/metadata
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
      // TODO: PATCH /videos/{id}/metadata
      // this.video.privacyStatus = this.editPrivacy;
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

    const payload = {
      title: this.editTitle || this.video.title,
      description: this.editDescription || this.video.description,
      privacyStatus: this.editPrivacy || this.video.privacyStatus
    };

    // this.videoService.updateVideo(this.video.guid, payload).subscribe({
    //   next: (res) => {
    //     this.video = res;
    //     this.hasUnsavedChanges = false;
    //     this.editingTitle = false;
    //     this.editingDescription = false;
    //     this.editingPrivacy = false;
    //   },
    //   error: (err) => {
    //     console.error(err);
    //     alert('儲存失敗，請稍後再試');
    //   }
    // });

    // 示意：更新成功
    this.hasUnsavedChanges = false;
    this.editingTitle = false;
    this.editingDescription = false;
    this.editingPrivacy = false;
    alert('已儲存所有變更（示意）');
  }
}
