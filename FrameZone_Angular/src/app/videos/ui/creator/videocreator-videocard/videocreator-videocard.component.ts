import { Component, Input } from '@angular/core';
import { VideoDurationPipe } from "../../../pipes/video-duration.pipe";
import { VideoTimeagoPipe } from "../../../pipes/video-timeago.pipe";
import { VideoDetailData } from '../../../models/videocreator-model';
import { PrivacyStatus, ProcessStatus } from '../../../models/video.enum';
import { NgClass } from '@angular/common';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
@Component({
  selector: 'app-videocreator-videocard',
  imports: [DatePipe, VideoDurationPipe, VideoTimeagoPipe, NgClass],
  templateUrl: './videocreator-videocard.component.html',
  styleUrl: './videocreator-videocard.component.css'
})
export class VideocreatorVideocardComponent {
  // 將 enum 暴露給模板使用
  ProcessStatus = ProcessStatus;
  PrivacyStatus = PrivacyStatus;

  @Input() Video: VideoDetailData = {
    id: 0,
    title: '',
    description: '',
    thumbnail: '',
    duration: 0,
    publishDate: new Date(),
    viewsCount: 0,
    likesCount: 0,
    commentCount: 0,
    videoUrl: '',
    processStatus: ProcessStatus.UPLOADING,
    privacyStatus: PrivacyStatus.PUBLIC
  }

  constructor(private router: Router, private route: ActivatedRoute) { }

  ngOnInit(): void {
    if (!this.Video.thumbnail) {
      this.Video.thumbnail =
        'https://localhost:7213/api/Videos/video-thumbnail/' + this.Video.videoUrl;
    }
  }

  onVideoClick(): void {
    console.log('Video clicked:', this.Video.id, 'guid:', this.Video.videoUrl);
    this.router.navigate(['/videos/videocreator/videoedit', this.Video.videoUrl]);

  }

  // 取得流程狀態顯示文字
  getProcessStatusText(): string {
    const statusMap: Record<ProcessStatus, string> = {
      [ProcessStatus.UPLOADING]: '上傳中',
      [ProcessStatus.UPLOADED]: '已上傳',
      [ProcessStatus.PRE_PROCESSING]: '前處理中',
      [ProcessStatus.TRANSCODING]: '轉碼中',
      [ProcessStatus.AI_AUDITING]: 'AI審核中',
      [ProcessStatus.READY]: '已就緒',
      [ProcessStatus.PUBLISHED]: '已發佈',
      [ProcessStatus.FAILED_TRANSCODE]: '轉碼失敗',
      [ProcessStatus.FAILED_AUDIT]: '審核未過'
    };
    return statusMap[this.Video.processStatus] || '';
  }

  // 取得隱私狀態顯示文字
  getPrivacyStatusText(): string {
    const statusMap: Record<PrivacyStatus, string> = {
      [PrivacyStatus.PUBLIC]: '公開',
      [PrivacyStatus.UNLISTED]: '非公開',
      [PrivacyStatus.PRIVATE]: '私人',
      [PrivacyStatus.DRAFT]: '草稿'
    };
    return statusMap[this.Video.privacyStatus] || '';
  }
}
