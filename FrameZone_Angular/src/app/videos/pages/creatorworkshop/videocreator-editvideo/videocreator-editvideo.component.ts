import { Component, HostListener } from '@angular/core';
import { NgClass, NgIf, NgForOf } from "@angular/common";
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-videocreator-editvideo',
  imports: [DatePipe, NgClass, NgIf, NgForOf, FormsModule],
  templateUrl: './videocreator-editvideo.component.html',
  styleUrl: './videocreator-editvideo.component.css'
})
export class VideocreatorEditvideoComponent {
  editThumbnail() {
    throw new Error('Method not implemented.');
  }
  // ====== 假資料 ======
  video = {
    videoId: 1,
    title: 'Angular 示範影片',
    description: '這是一段示範用的影片描述\n支援換行顯示',
    videoUrl: 'https://www.w3schools.com/html/mov_bbb.mp4',
    thumbnailUrl: 'https://picsum.photos/800/450',
    channelId: 'CHANNEL_001',
    processStatus: 'Completed',
    privacyStatus: 'Public',
    duration: 245,
    resolution: '1920x1080',
    fileSize: 734003200,
    createdAt: new Date(),
    updatedAt: new Date()
  };

  viewCount = 12345;
  likeCount = 678;
  commentCount = 89;


  hasUnsavedChanges = false; // 判斷是否有修改
  // ===== Inline Edit 狀態 =====
  editingTitle = false;
  editingDescription = false;
  editingPrivacy = false;

  editTitle = '';
  editDescription = '';
  editPrivacy = '';


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
    if (size >= 1024 ** 3) return (size / 1024 ** 3).toFixed(2) + ' GB';
    if (size >= 1024 ** 2) return (size / 1024 ** 2).toFixed(2) + ' MB';
    if (size >= 1024) return (size / 1024).toFixed(2) + ' KB';
    return size + ' B';
  }

  getDuration(seconds: number): string {
    const m = Math.floor(seconds / 60).toString().padStart(2, '0');
    const s = Math.floor(seconds % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  }

  deleteVideo() {
    alert('刪除影片（示意）');
  }


  // ===== Title =====
  startEditTitle() {
    this.hasUnsavedChanges = true;
    this.editTitle = this.video.title;
    this.editingTitle = true;
  }

  saveTitle() {
    if (this.editTitle !== this.video.title) {
      // TODO PATCH /videos/{id}/metadata
      this.video.title = this.editTitle;
    }
    this.editingTitle = false;
  }

  cancelEditTitle() {
    this.editingTitle = false;
  }

  // ===== Description =====
  startEditDescription() {
    this.hasUnsavedChanges = true;
    this.editDescription = this.video.description;
    this.editingDescription = true;
  }

  cancelEditDescription() {
    this.editingDescription = false;
  }

  saveDescription() {
    if (this.editDescription !== this.video.description) {
      // TODO PATCH
      this.video.description = this.editDescription;
    }
    this.editingDescription = false;
  }

  // ===== Privacy =====
  startEditPrivacy() {
    if (!this.canEditPrivacy()) return;
    this.hasUnsavedChanges = true;
    this.editPrivacy = this.video.privacyStatus;
    this.editingPrivacy = true;
  }

  savePrivacy() {
    if (this.editPrivacy !== this.video.privacyStatus) {
      // TODO PATCH
      this.video.privacyStatus = this.editPrivacy;
    }
    this.editingPrivacy = false;
  }

  cancelEditPrivacy() {
    this.editingPrivacy = false;
  }

  canEditPrivacy(): boolean {
    return this.video.processStatus === 'Completed';
  }

  //================監聽改變
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    if (this.hasUnsavedChanges) {
      $event.returnValue = true;
    }
  }
  //================影片儲存
  saveAllChanges() {
    const payload = {
      title: this.editTitle,
      description: this.editDescription,
      privacyStatus: this.editPrivacy
    };

    // this.videoService.updateVideo(this.video.guid, payload).subscribe({
    //   next: (res) => {
    //     this.video = res;          // 更新畫面
    //     this.hasUnsavedChanges = false; // 隱藏警示
    //     this.editingTitle = false;
    //     this.editingDescription = false;
    //     this.editingPrivacy = false;
    //   },
    //   error: (err) => {
    //     console.error(err);
    //     alert('儲存失敗，請稍後再試');
    //   }
    // });
  }

  // ======================
  // Util
  // ======================
  formatDuration(seconds: number): string {
    const m = Math.floor(seconds / 60).toString().padStart(2, '0');
    const s = Math.floor(seconds % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  }


}
