import { FormsModule } from '@angular/forms';
import { Component, Input } from '@angular/core';
import { VideoPlayerComponent } from '../../../ui/video/video-player/video-player.component';
import { VideoTimeagoPipe } from "../../../pipes/video-timeago.pipe";
import { VideoActionsBarComponent } from "../../../ui/actions/video-actions-bar/video-actions-bar.component";
import { ChannelCardComponent } from "../../../ui/channel/channel-card/channel-card.component";
import { NgIf } from '@angular/common';
import { VideoCommentListComponent } from "../../../ui/comments/video-comment-list/video-comment-list.component";
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { ChannelCard, VideoCardData, VideoCommentCard, VideoCommentRequest } from '../../../models/video-model';
import { ActivatedRoute } from '@angular/router';
import { TargetTypeEnum } from '../../../models/video.enum';
import { VideoService } from '../../../service/video.service';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-video-main',
  imports: [FormsModule, VideoPlayerComponent, VideoTimeagoPipe, VideoActionsBarComponent, ChannelCardComponent, NgIf, VideoCommentListComponent, VideosListComponent],
  templateUrl: './video-main.component.html',
  styleUrl: './video-main.component.css'
})
export class VideoMainComponent {

  channel: ChannelCard = {
    id: 1,
    Name: '頻道名稱示例',
    Avatar: 'https://i.pravatar.cc/48',
    Description: "這個人很懶，甚麼都沒留",
    Follows: 12345,
  };

  commentList: VideoCommentCard[] = []; // 留言列表

  /* =====================================================
 * 📌 基本資料（影片 / 路由）
 * ===================================================== */

  //videoid
  videoid: number = 0;

  /** 當前播放的影片資料 */
  video: VideoCardData | null = null;

  /** 推薦影片列表（由父層傳入） */
  @Input() videosRecommand?: VideoCardData[];

  /** 影片 GUID（從路由取得） */
  guid: string | null = null;

  /** 實際影片播放來源（HLS / MP4） */
  videoUrl: string = '';


  /* =====================================================
   * 🎬 播放器 & 畫面狀態
   * ===================================================== */

  /** 影片是否已載入（用於動畫或骨架） */
  isVideoLoaded = false;

  /** 播放器 hover 狀態（顯示控制列等） */
  isPlayerHovered = false;


  /* =====================================================
   * 📝 影片描述顯示狀態
   * ===================================================== */

  /** 描述是否展開 */
  isDescriptionExpanded = false;

  /** 是否顯示「展開更多」按鈕 */
  showExpandButton = false;

  /** 描述顯示最大長度 */
  private readonly MAX_DESCRIPTION_LENGTH = 200;


  /* =====================================================
   * 💬 留言相關狀態
   * ===================================================== */

  //使用者id
  currentUserId: number = 1; // 模擬登入用
  /** 使用者正在輸入的留言 */
  newComment: string = '';

  /** 是否正在送出留言（避免重複送出） */
  isSubmitting = false;

  /** 使用者頭像字母（之後可從登入資訊取得） */
  currentUserInitial = 'I';


  /* =====================================================
   * 🔧 DI
   * ===================================================== */

  constructor(
    private route: ActivatedRoute,
    private videoService: VideoService, private cdr: ChangeDetectorRef
  ) { }


  /* =====================================================
   * 🚀 Lifecycle
   * ===================================================== */

  ngOnInit(): void {

    /* 1️⃣ 取得路由中的影片 GUID */
    this.guid = this.route.snapshot.paramMap.get('guid');
    if (!this.guid) return;

    /* 2️⃣ 取得影片資料 */
    this.loadVideoData(this.guid);

    /* 3️⃣ 設定播放器來源 */
    this.setVideoSource(this.guid);

    //讀取留言
    this.videoService.getVideoComments(this.guid).subscribe({
      next: (comments: VideoCommentCard[]) => {
        this.commentList = comments; // 這裡才是陣列
      },
      error: (err) => console.error(err)
    });

    /* 4️⃣ 模擬影片載入完成（UI 動畫用） */
    setTimeout(() => {
      this.isVideoLoaded = true;
      this.cdr.detectChanges(); // 強制檢查變更，避免錯誤
    }, 300);
  }


  /* =====================================================
   * 🎥 影片相關方法
   * ===================================================== */

  /**
   * 取得影片詳細資料
   */
  private loadVideoData(guid: string): void {
    this.videoService.getVideo(guid).subscribe({
      next: (data) => {
        this.video = data;
        console.log('影片資料:', this.video);

        // 檢查描述是否需要「展開」
        if (this.video?.description &&
          this.video.description.length > this.MAX_DESCRIPTION_LENGTH) {
          this.showExpandButton = true;
        }
      },
      error: (err) => {
        console.error('取得影片資料失敗', err);
      }
    });
  }

  /**
   * 設定播放器影片來源
   */
  private setVideoSource(guid: string): void {
    this.videoUrl = `https://localhost:7213/api/videoplayer/${guid}`;
  }


  /* =====================================================
   * 📝 影片描述顯示
   * ===================================================== */

  toggleDescription(): void {
    this.isDescriptionExpanded = !this.isDescriptionExpanded;
  }

  /**
   * 取得實際要顯示的描述內容
   */
  getDisplayDescription(): string {
    if (!this.video?.description) return '';

    if (this.isDescriptionExpanded ||
      this.video.description.length <= this.MAX_DESCRIPTION_LENGTH) {
      return this.video.description;
    }

    return this.video.description.substring(0, this.MAX_DESCRIPTION_LENGTH) + '...';
  }


  /* =====================================================
   * 🖱️ 播放器互動
   * ===================================================== */

  onPlayerHover(state: boolean): void {
    this.isPlayerHovered = state;
  }


  /* =====================================================
   * 💬 留言相關方法
   * ===================================================== */

  /**
   * 送出留言（目前為前端佔位）
   * 之後可接後端 API
   */
  submitComment(parentId?: number): void {
    if (!this.newComment.trim()) return;

    this.isSubmitting = true;

    const req: VideoCommentRequest = {
      UserId: this.currentUserId,
      VideoId: Number(this.video?.videoId),
      TargetTypeId: TargetTypeEnum.Video,
      CommentContent: this.newComment,
      ParentCommentId: parentId,
    };

    this.videoService.postVideoComment(req).subscribe({
      next: (res) => {
        this.commentList.unshift(res); // 置頂新留言
        this.newComment = '';
        this.isSubmitting = false;
      },
      error: () => {
        console.error('留言失敗');
        this.isSubmitting = false;
      }
    });

  }

  submitReply(event: { parentId: number; message: string }) {
    const req: VideoCommentRequest = {
      UserId: this.currentUserId,
      VideoId: Number(this.video?.videoId),
      TargetTypeId: TargetTypeEnum.Video,
      CommentContent: event.message,
      ParentCommentId: event.parentId // ✅ 父留言 ID
    };

    this.videoService.postVideoComment(req).subscribe({
      next: (res) => {
        const parent = this.commentList.find(c => c.id === event.parentId);
        if (parent) {
          parent.replies = parent.replies || [];
          parent.replies.unshift(res);
        }
      },
      error: () => console.error('回覆留言失敗')
    });
  }
}
