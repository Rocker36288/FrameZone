import { TargetTypeEnum } from "./video.enum";

// 影片卡片資訊
export class VideoCardData {
  // ⚠️ 僅供內部使用（例如 trackBy）
  id?: number;

  // 後端回傳的 videoId
  videoId?: number;

  videoUri: string = '';
  title: string = '';
  thumbnail: string = '';
  duration: number = 0;
  views: number = 0;
  publishDate?: Date;
  description: string = '';
  likes: number = 0;

  channelId: number = 0;
  channelName: string = '';
  avatar: string = '';

  constructor(data?: Partial<VideoCardData>) {
    if (data) {
      Object.assign(this, data);
    }
  }
}

//影片撥放清單表
export class VideoListCard {
  Id: number = 0;
  Title: string = '';
  Description: string = '';
  VideoCount: number = 0;
  thumbnail: string = ''; //一般取第一個影片的小圖示
  constructor(data?: Partial<VideoListCard>) {
    if (data) {
      Object.assign(this, data); // 如果傳入了部分資料，就用來覆蓋預設值
    }
  }
}

//留言卡片
export class VideoCommentCard {
  id: number = 0;
  userName: string = '';
  avatar: string = '';
  message: string = '';
  createdAt: Date = new Date();
  likes: number = 0;
  replies?: VideoCommentCard[] = [];

  constructor(data?: Partial<VideoCommentCard>) {
    if (data) {
      Object.assign(this, data); // 如果傳入了部分資料，就用來覆蓋預設值
    }
  }
}

//頻道卡片資訊
export interface ChannelCard {
  id: number;
  name: string;
  avatar: string | null;
  description: string;
  follows: number;
}

//頻道主頁資訊
export class ChannelHome {
  id: number = 0;
  name: string = '';
  avatar: string = '';
  description: string = '';
  follows: number = 0;
  videosCount: number = 0;
  banner: string = ''; //橫幅圖片位置
  createdAt = new Date(); //頻道建立日期
  lastUpdateAt = new Date(); //最後一次上船影片日期


  constructor(data?: Partial<ChannelHome>) {
    if (data) {
      Object.assign(this, data); // 如果傳入了部分資料，就用來覆蓋預設值
    }
  }
}

export interface VideoCommentRequest {
  //UserId: number;          // long → number
  VideoId: number;         // int → number（注意命名）
  TargetTypeId: TargetTypeEnum.Video;    // enum / int
  CommentContent: string;
  ParentCommentId: number | undefined;
}

//likeDto
export interface VideoLikesRequest {
  isLikes: boolean;
  videoId: number;
}

export interface VideoLikesDto {
  isLikes: boolean;
}

//viewHistoryDto
export interface UpdateWatchHistoryRequest {
  videoId: number;
  lastPosition: number;
}

export interface VideoWatchHistoryDto {
  video: VideoCardData;
  lastPosition: number; // 已看秒數
  lastWatchedAt: string;
}

export interface VideoSearchParams {
  keyword?: string;
  sortBy?: 'likes' | 'views' | 'date';
  sortOrder?: 'asc' | 'desc';
  take?: number;
}


export interface ChannelSpotlightDto {
  channel: ChannelCard;
  videos: VideoCardData[];
}
