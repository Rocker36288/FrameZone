export class VideoCardData {
  id: number = 0;
  title: string = '';
  thumbnail: string = '';
  duration: number = 0; // 預設為0秒
  views: number = 0;
  uploadDate: Date = new Date();
  description: string = '';

  channelName: string = '';
  Avatar: string = '';

  constructor(data?: Partial<VideoCardData>) {
    if (data) {
      Object.assign(this, data); // 如果傳入了部分資料，就用來覆蓋預設值
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
export class VideoCommentModel {
  id: number = 0;
  userName: string = '';
  avatar: string = '';
  message: string = '';
  createdAt: Date = new Date();
  likes: number = 0;
  replies?: VideoCommentModel[] = [];

  constructor(data?: Partial<VideoCommentModel>) {
    if (data) {
      Object.assign(this, data); // 如果傳入了部分資料，就用來覆蓋預設值
    }
  }
}

//頻道卡片資訊
export class ChannelCard {
  id: number = 0;
  Name: string = '';
  Avatar: string = '';
  Description: string = '';
  Follows: number = 0;

  constructor(data?: Partial<ChannelCard>) {
    if (data) {
      Object.assign(this, data); // 如果傳入了部分資料，就用來覆蓋預設值
    }
  }
}

//頻道主頁資訊
export class ChannelHome {
  id: number = 0;
  Name: string = '';
  Avatar: string = '';
  Description: string = '';
  Follows: number = 0;
  VideosCount: number = 0;
  Banner: string = ''; //橫幅圖片位置
  CreatedAt = new Date(); //頻道建立日期
  LastUpdateAt = new Date(); //最後一次上船影片日期


  constructor(data?: Partial<ChannelHome>) {
    if (data) {
      Object.assign(this, data); // 如果傳入了部分資料，就用來覆蓋預設值
    }
  }
}
