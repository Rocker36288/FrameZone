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
    Name: '頻道名稱示例',
    Avatar: 'https://i.pravatar.cc/48',
    Description: "他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下",
    Follows: 12345,
  };

  video: VideoCardData = {
    id: 5,
    Avatar: '',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 2158,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明"
  };

  videos: VideoCardData[] = [{
    id: 1,
    Avatar: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 2158,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    Avatar: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    Avatar: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    Avatar: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    Avatar: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    Avatar: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }
  ];

  VideoPlaylists: VideoListCard[] = [{
    Id: 0,
    Title: 'DDD',
    Description: 'FF',
    VideoCount: 12,
    thumbnail: 'https://picsum.photos/480/270'
  }]

  Channelhome: ChannelHome = {
    id: 0,
    Name: 'RR',
    Avatar: 'https://i.pravatar.cc/49',
    Description: 'FFFF',
    Follows: 15,
    VideosCount: 18,
    Banner: '',
    CreatedAt: new Date('2002-02-07'),
    LastUpdateAt: new Date('2002-02-07')
  }

  VideoDetailData: VideoDetailData = {
    id: 0,
    title: '',
    description: '',
    thumbnail: '',
    duration: 0,
    uploadDate: new Date(),
    viewsCount: 0,
    likesCount: 0,
    commentCount: 0,
    videoUrl: '',
    processStatus: ProcessStatus.UPLOADING,
    privacyStatus: PrivacyStatus.PUBLIC
  }

  VideoDetailsData: VideoDetailData[] = [{
    id: 0,
    title: '',
    description: '',
    thumbnail: '',
    duration: 0,
    uploadDate: new Date(),
    viewsCount: 15,
    likesCount: 0,
    commentCount: 0,
    videoUrl: '',
    processStatus: ProcessStatus.UPLOADING,
    privacyStatus: PrivacyStatus.PUBLIC
  }, {
    id: 0,
    title: '',
    description: '',
    thumbnail: '',
    duration: 0,
    uploadDate: new Date(),
    viewsCount: 15,
    likesCount: 0,
    commentCount: 0,
    videoUrl: '',
    processStatus: ProcessStatus.UPLOADING,
    privacyStatus: PrivacyStatus.PUBLIC
  }, {
    id: 0,
    title: '',
    description: '',
    thumbnail: '',
    duration: 0,
    uploadDate: new Date(),
    viewsCount: 15,
    likesCount: 0,
    commentCount: 0,
    videoUrl: '',
    processStatus: ProcessStatus.UPLOADING,
    privacyStatus: PrivacyStatus.PUBLIC
  }]

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
