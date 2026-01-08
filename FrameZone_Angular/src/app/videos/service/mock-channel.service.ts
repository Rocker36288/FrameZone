import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { ChannelCard, ChannelHome, VideoCardData, VideoListCard } from '../models/video-model';
import { VideoDetailData } from '../models/videocreator-model';
import { PrivacyStatus, ProcessStatus } from '../models/video.enum';

@Injectable({
  providedIn: 'root'
})
export class MockChannelService {
  channel: ChannelCard = {
    id: 1,
    name: '光影攝影室 LightFrame',
    avatar: 'https://picsum.photos/seed/photo-channel/256/256',
    description:
      '專注於人像、風景與街拍攝影教學，分享構圖技巧、器材評測與後製流程。' +
      '不論你是新手還是進階攝影師，都能在這裡找到靈感。',
    follows: 26840
  };

  video: VideoCardData = {
    id: 10,
    avatar: 'https://picsum.photos/seed/photographer/48/48',
    title: '【攝影教學】掌握光線的 5 個關鍵技巧｜新手必看',
    channelName: '光影攝影室',
    thumbnail: 'https://picsum.photos/seed/photo-main/1280/720',
    duration: 1420,
    views: 48213,
    publishDate: new Date('2024-11-05'),
    description:
      '本影片將教你如何運用自然光與人造光源，' +
      '拍出更有層次與情緒的照片，適合所有攝影新手。',
    videoUri: '',
    channelId: 1,
    likes: 1342
  };


  videos: VideoCardData[] = [
    {
      id: 1,
      avatar: 'https://picsum.photos/seed/p1/48/48',
      title: '新手必學｜攝影構圖 7 大原則',
      channelName: '光影攝影室',
      thumbnail: 'https://picsum.photos/seed/photo1/1280/720',
      duration: 980,
      views: 32145,
      publishDate: new Date('2024-09-10'),
      description: '從三分法到引導線，讓照片瞬間升級。',
      videoUri: '',
      channelId: 1,
      likes: 856
    },
    {
      id: 2,
      avatar: 'https://picsum.photos/seed/p2/48/48',
      title: '人像攝影打光技巧｜自然光 vs 補光燈',
      channelName: '光影攝影室',
      thumbnail: 'https://picsum.photos/seed/photo2/1280/720',
      duration: 1560,
      views: 28760,
      publishDate: new Date('2024-09-28'),
      description: '教你在室內與戶外拍出自然好看的人像。',
      videoUri: '',
      channelId: 1,
      likes: 1043
    },
    {
      id: 3,
      avatar: 'https://picsum.photos/seed/p3/48/48',
      title: '風景攝影技巧｜拍出有層次的山景',
      channelName: '光影攝影室',
      thumbnail: 'https://picsum.photos/seed/photo3/1280/720',
      duration: 1320,
      views: 21984,
      publishDate: new Date('2024-10-12'),
      description: '從構圖、曝光到後製一次講清楚。',
      videoUri: '',
      channelId: 1,
      likes: 732
    },
    {
      id: 4,
      avatar: 'https://picsum.photos/seed/p4/48/48',
      title: '攝影新手該買哪顆鏡頭？',
      channelName: '光影攝影室',
      thumbnail: 'https://picsum.photos/seed/photo4/1280/720',
      duration: 1180,
      views: 40322,
      publishDate: new Date('2024-11-03'),
      description: '定焦 vs 變焦鏡，一次幫你分析清楚。',
      videoUri: '',
      channelId: 1,
      likes: 1620
    },
    {
      id: 5,
      avatar: 'https://picsum.photos/seed/p5/48/48',
      title: 'Lightroom 修圖流程完整教學',
      channelName: '光影攝影室',
      thumbnail: 'https://picsum.photos/seed/photo5/1280/720',
      duration: 1740,
      views: 35590,
      publishDate: new Date('2024-12-01'),
      description: '從原圖到成品的完整修圖示範。',
      videoUri: '',
      channelId: 1,
      likes: 1893
    }
  ];

  Videos2: VideoCardData[] = [
    {
      id: 101,
      avatar: 'https://picsum.photos/seed/tech1/48/48',
      title: '2025 最值得買的攝影手機 TOP 5',
      channelName: '極客實驗室',
      thumbnail: 'https://picsum.photos/seed/photo1/1280/720',
      duration: 1020,
      views: 84521,
      publishDate: new Date('2025-01-10'),
      description: '從拍照、錄影到夜拍，完整實測比較。',
      videoUri: '',
      channelId: 2,
      likes: 2314
    },
    {
      id: 102,
      avatar: 'https://picsum.photos/seed/tech2/48/48',
      title: '全片幅 vs APS-C 差在哪？',
      channelName: '極客實驗室',
      thumbnail: 'https://picsum.photos/seed/tech-video2/1280/720',
      duration: 890,
      views: 45672,
      publishDate: new Date('2025-01-18'),
      description: '一次搞懂感光元件尺寸的差異。',
      videoUri: '',
      channelId: 2,
      likes: 1783
    },
    {
      id: 201,
      avatar: 'https://picsum.photos/seed/travel1/48/48',
      title: '東京街拍一日｜用鏡頭記錄城市節奏',
      channelName: '城市漫遊者',
      thumbnail: 'https://picsum.photos/seed/travel-video1/1280/720',
      duration: 1430,
      views: 62340,
      publishDate: new Date('2024-11-22'),
      description: '走訪澀谷、原宿，捕捉最真實的街頭瞬間。',
      videoUri: '',
      channelId: 3,
      likes: 2105
    },
    {
      id: 202,
      avatar: 'https://picsum.photos/seed/travel2/48/48',
      title: '旅行攝影必帶 5 樣裝備',
      channelName: '城市漫遊者',
      thumbnail: 'https://picsum.photos/seed/photo4/1280/720',
      duration: 760,
      views: 33890,
      publishDate: new Date('2024-12-08'),
      description: '輕裝出門也能拍出高質感照片。',
      videoUri: '',
      channelId: 3,
      likes: 1294
    }, {
      id: 301,
      avatar: 'https://picsum.photos/seed/edit1/48/48',
      title: 'Lightroom 色調調整完整教學',
      channelName: '影像後製所',
      thumbnail: 'https://picsum.photos/seed/photo3/1280/720',
      duration: 1680,
      views: 51200,
      publishDate: new Date('2024-10-30'),
      description: '教你調出電影感色調的關鍵技巧。',
      videoUri: '',
      channelId: 4,
      likes: 2450
    },
    {
      id: 302,
      avatar: 'https://picsum.photos/seed/edit2/48/48',
      title: 'Photoshop 修圖流程大公開',
      channelName: '影像後製所',
      thumbnail: 'https://picsum.photos/seed/edit-video2/1280/720',
      duration: 1920,
      views: 47120,
      publishDate: new Date('2024-11-18'),
      description: '從 RAW 檔到成品的完整流程示範。',
      videoUri: '',
      channelId: 4,
      likes: 2890
    }
  ];

  Videos3: VideoCardData[] = [{
    id: 401,
    avatar: 'https://picsum.photos/seed/wild1/48/48',
    title: '野生動物攝影入門｜拍到不被發現的秘訣',
    channelName: '野境攝影 Wildlife Lens',
    thumbnail: 'https://picsum.photos/seed/wildlife1/1280/720',
    duration: 1480,
    views: 56230,
    publishDate: new Date('2024-10-05'),
    description: '教你如何在不干擾動物的情況下拍出自然畫面。',
    videoUri: '',
    channelId: 5,
    likes: 1834
  },
  {
    id: 402,
    avatar: 'https://picsum.photos/seed/wild2/48/48',
    title: '野鳥攝影技巧｜對焦與快門設定全解析',
    channelName: '野境攝影 Wildlife Lens',
    thumbnail: 'https://picsum.photos/seed/wildlife2/1280/720',
    duration: 1720,
    views: 44712,
    publishDate: new Date('2024-10-22'),
    description: '高速快門與追焦設定一次搞懂。',
    videoUri: '',
    channelId: 5,
    likes: 2011
  },
  {
    id: 403,
    avatar: 'https://picsum.photos/seed/wild3/48/48',
    title: '長焦鏡頭實拍測試｜拍攝野生動物必備',
    channelName: '野境攝影 Wildlife Lens',
    thumbnail: 'https://picsum.photos/seed/wildlife3/1280/720',
    duration: 1360,
    views: 38904,
    publishDate: new Date('2024-11-14'),
    description: '不同焦段實拍比較，選鏡不再迷惘。',
    videoUri: '',
    channelId: 5,
    likes: 1657
  }, {
    id: 501,
    avatar: 'https://picsum.photos/seed/pet1/48/48',
    title: '如何幫狗狗拍出自然可愛的照片',
    channelName: '毛孩日常 Pet Shot',
    thumbnail: 'https://picsum.photos/seed/petphoto1/1280/720',
    duration: 940,
    views: 61230,
    publishDate: new Date('2024-09-18'),
    description: '掌握角度與光線，拍出最療癒的瞬間。',
    videoUri: '',
    channelId: 6,
    likes: 2741
  },
  {
    id: 502,
    avatar: 'https://picsum.photos/seed/pet2/48/48',
    title: '貓咪攝影技巧｜抓住最萌瞬間',
    channelName: '毛孩日常 Pet Shot',
    thumbnail: 'https://picsum.photos/seed/petphoto2/1280/720',
    duration: 860,
    views: 53320,
    publishDate: new Date('2024-10-06'),
    description: '貓咪拍攝時機與構圖技巧分享。',
    videoUri: '',
    channelId: 6,
    likes: 3120
  },
  {
    id: 503,
    avatar: 'https://picsum.photos/seed/pet3/48/48',
    title: '寵物寫真棚拍流程公開',
    channelName: '毛孩日常 Pet Shot',
    thumbnail: 'https://picsum.photos/seed/petphoto3/1280/720',
    duration: 1540,
    views: 29840,
    publishDate: new Date('2024-11-20'),
    description: '從打光到引導寵物動作的完整流程。',
    videoUri: '',
    channelId: 6,
    likes: 1987
  }]

  VideoPlaylists: VideoListCard[] = [{
    Id: 0,
    Title: 'DDD',
    Description: 'FF',
    VideoCount: 12,
    thumbnail: 'https://picsum.photos/480/270'
  }]

  Channelhome: ChannelHome = {
    id: 0,
    name: 'RR',
    avatar: 'https://i.pravatar.cc/49',
    description: 'FFFF',
    follows: 15,
    videosCount: 18,
    banner: '',
    createdAt: new Date('2002-02-07'),
    lastUpdateAt: new Date('2002-02-07')
  }


  VideoDetailsData: VideoDetailData[] | undefined

  getvideos2() {
    return of(this.Videos2)
  }

  getChannelCard() {
    return of(this.channel);
  }

  getChannelHome() {
    return of(this.Channelhome);
  }

  getChannelVideos() {
    return of(this.videos);
  }

  getChannelPlaylists() {
    return of(this.VideoPlaylists);
  }

  getVideoDetailsData() {
    return of(this.VideoDetailsData);
  }


}
