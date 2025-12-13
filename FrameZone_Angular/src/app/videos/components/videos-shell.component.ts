import { Component } from '@angular/core';
import { HeaderComponent } from "../../shared/components/header/header.component";
import { VideosSidebarComponent } from "../ui/videos-sidebar/videos-sidebar.component";
import { ChannelData } from '../models/channel-data.interface';
import { VideoCardData } from '../models/video-data.interface';
import { VideoHomeComponent } from "./home/video-home/video-home.component";

@Component({
  selector: 'app-videos-shell',
  imports: [HeaderComponent, VideosSidebarComponent, VideoHomeComponent],
  templateUrl: './videos-shell.component.html',
  styleUrl: './videos-shell.component.css'
})
export class VideosShellComponent {

  channel: ChannelData = {
    id: 1,
    Name: '頻道名稱示例',
    Avatar: 'https://i.pravatar.cc/48',
    Description: "他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下他很懶甚麼都沒留下",
    Follows: 12345,
  };

  video: VideoCardData = {
    id: 5,
    userAvatarUrl: '',
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
    userAvatarUrl: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 2158,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    userAvatarUrl: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    userAvatarUrl: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    userAvatarUrl: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    userAvatarUrl: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    userAvatarUrl: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }
  ];
}
