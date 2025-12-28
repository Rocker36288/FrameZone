import { AuthService } from './../../../../core/services/auth.service';
import { FormsModule } from '@angular/forms';
import { Component, Input } from '@angular/core';
import { VideoPlayerComponent } from '../../../ui/video/video-player/video-player.component';
import { VideoActionsBarComponent } from "../../../ui/actions/video-actions-bar/video-actions-bar.component";
import { ChannelCardComponent } from "../../../ui/channel/channel-card/channel-card.component";
import { NgIf } from '@angular/common';
import { VideoCommentListComponent } from "../../../ui/comments/video-comment-list/video-comment-list.component";
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { ChannelCard, VideoCardData, VideoCommentCard, VideoCommentRequest, VideoLikesDto, VideoLikesRequest } from '../../../models/video-model';
import { ActivatedRoute } from '@angular/router';
import { TargetTypeEnum } from '../../../models/video.enum';
import { VideoService } from '../../../service/video.service';
import { ChangeDetectorRef } from '@angular/core';
import { VideosSidebarComponent } from "../../../ui/videos-sidebar/videos-sidebar.component";
import { DatePipe } from '@angular/common';
import { VideoSearchComponent } from "../../search/video-search/video-search.component";
import { SearchboxComponent } from "../../../ui/searchbox/searchbox.component";
import { MockChannelService } from '../../../service/mock-channel.service';
import { CommonModule } from '@angular/common';
import { VideosSharedModalComponent } from "../../../ui/videos-shared-modal/videos-shared-modal.component";
import { VideosNotloginyetModalComponent } from "../../../ui/videos-notloginyet-modal/videos-notloginyet-modal.component";

@Component({
  selector: 'app-video-main',
  imports: [CommonModule, DatePipe, FormsModule, VideoPlayerComponent, VideoActionsBarComponent, ChannelCardComponent, NgIf, VideoCommentListComponent, VideosListComponent, VideosSidebarComponent, VideoSearchComponent, SearchboxComponent, VideosSharedModalComponent, VideosNotloginyetModalComponent],
  templateUrl: './video-main.component.html',
  styleUrl: './video-main.component.css'
})
export class VideoMainComponent {

  channel: ChannelCard | undefined

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

  //是否喜歡
  isLiked: boolean = false;

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

  //===============
  showLoginModal = false; // 控制 Modal 顯示

  userLoggedIn = false; // 假設是否登入


  /* =====================================================
   * 🔧 DI
   * ===================================================== */

  constructor(
    private route: ActivatedRoute,
    private videoService: VideoService, private cdr: ChangeDetectorRef,
    private mockChannelService: MockChannelService
    , private authService: AuthService
  ) { }


  /* =====================================================
   * 🚀 Lifecycle
   * ===================================================== */


  ngOnInit(): void {

    /* 1️⃣ 取得路由 GUID */
    const guid = this.route.snapshot.paramMap.get('guid');
    if (!guid) return;
    this.guid = guid;

    /* 2️⃣ 載入影片（所有後續行為從這裡開始） */
    this.loadVideoData(guid);

    /* 3️⃣ 載入推薦影片（與影片本身無依賴） */
    this.loadRecommendVideos();

    // 檢測是否有登入
    if (this.authService.currentUser$) {
      this.checkLikeStatus()
    }

    /* 4️⃣ UI 動畫 */
    setTimeout(() => {
      this.isVideoLoaded = true;
      this.cdr.detectChanges();
    }, 300);
  }

  /* ===============================
   📌 API 呼叫區
   =============================== */

  /** 載入影片資料 */
  private loadVideoData(guid: string): void {
    this.videoService.getVideo(guid).subscribe({
      next: (video) => {
        this.video = video;
        console.log('影片資料:', this.video);
        this.setVideoSource(guid);

        /* 1️⃣ 描述是否顯示「展開」按鈕 */
        if (
          this.video.description &&
          this.video.description.length > this.MAX_DESCRIPTION_LENGTH
        ) {
          this.showExpandButton = true;
        } else {
          this.showExpandButton = false;
        }

        /* 2️⃣ 影片一到，就該做的事（不依賴 description） */
        this.loadChannel(video.channelId);
        this.loadComments(guid);
      },
      error: err => console.error('取得影片失敗', err)
    });
  }

  /** 載入頻道卡片 */
  private loadChannel(channelId: number): void {
    this.videoService.getChannelCard(channelId).subscribe({
      next: (channel: ChannelCard) => {
        this.channel = channel;
        console.log('頻道資料', channel);
      },
      error: err => console.error('取得頻道失敗', err)
    });
  }

  /** 載入留言 */
  private loadComments(guid: string): void {
    this.videoService.getVideoComments(guid).subscribe({
      next: (comments: VideoCommentCard[]) => {
        this.commentList = comments;
      },
      error: err => console.error('取得留言失敗', err)
    });
  }

  /** 載入推薦影片 */
  private loadRecommendVideos(): void {
    this.videoService.getVideoRecommend().subscribe({
      next: () => {
        this.videosRecommand = [
          ...this.mockChannelService.videos,
          ...this.mockChannelService.Videos3
        ];
      },
      error: err => console.error('取得推薦影片失敗', err)
    });
  }


  /**
   * 設定播放器影片來源
   */
  private setVideoSource(guid: string): void {

    this.videoUrl = `https://localhost:7213/api/videoplayer/${guid}`;
    console.log(this.videoUrl)
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
      //UserId: this.currentUserId,
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
      //UserId: this.currentUserId,
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

  //====================like相關=====================
  checkLikeStatus() {
    this.videoService.getVideoLikes(this.guid!).subscribe({
      next: (res: VideoLikesDto) => {
        this.isLiked = res.isLikes;
      },
      error: (err) => console.error('檢測失敗', err)
    });
  }

  onLikeChanged(liked: boolean) {
    if (!this.CheckLogin()) return; // 未登入直接 return

    // ✅ 已登入才更新
    this.isLiked = liked;

    const req: VideoLikesRequest = {
      videoId: this.videoid,
      isLikes: !this.isLiked
    };

    this.videoService.ToggleVideoLikes(this.guid!, req).subscribe({
      next: (res: VideoLikesDto) => {
        this.isLiked = res.isLikes;
        this.video!.likes += this.isLiked ? 1 : -1;
      },
      error: (err) => console.error('按讚失敗', err)
    });
  }

  //=======分享===============
  showShare = false;

  openShare() {
    console.log('🔥 openShare called');
    this.showShare = true;
  }
  // ======登入檢測
  CheckLogin() {
    if (this.authService.getCurrentUser()) {
      return true
    } else {
      this.showLoginModal = true
      return false
    }
  }
}
