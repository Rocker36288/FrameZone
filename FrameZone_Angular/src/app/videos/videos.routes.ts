import { Routes } from '@angular/router';
import { VideosShellComponent } from './pages/videos-shell.component';
import { VideoMainComponent } from './pages/player/video-main/video-main.component';
import { ChannelLayoutComponent } from './pages/channel/channel-layout/channel-layout.component';
import { ChannelVideosComponent } from './pages/channel/channel-videos/channel-videos.component';
import { ChannelComponent } from './pages/channel/channel-home/channel-home.component';
import { ChannelPlaylistsComponent } from './pages/channel/channel-playlists/channel-playlists.component';
import { VideocreatorHomeComponent } from './pages/creatorworkshop/videocreator-home/videocreator-home.component';
import { VideocreatorVideomanageComponent } from './pages/creatorworkshop/videocreator-videomanage/videocreator-videomanage.component';
import { VideocreatorUploadComponent } from './pages/creatorworkshop/videocreator-upload/videocreator-upload.component';
import { VideocreatorLayoutComponent } from './pages/creatorworkshop/videocreator-layout/videocreator-layout.component';
import { videoGuard } from './guard/video.guard';
import { VideoHomeComponent } from './pages/home/video-home/video-home.component';

export const VIDEO_ROUTES: Routes = [
  {
    path: '',
    // 建議將 VideosShellComponent 作為頂層 Layout，包含導覽列與側邊欄
    component: VideosShellComponent,
    children: [
      // 預設首頁
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: VideoHomeComponent }, // 註：如果 Shell 本身就是 Home，這裡可調整

      // 影片播放頁
      { path: 'watch/:guid', component: VideoMainComponent },

      // 頻道頁面（巢狀路由）
      {
        path: 'channel/:id',
        component: ChannelLayoutComponent,
        children: [
          { path: '', redirectTo: 'home', pathMatch: 'full' },
          { path: 'home', component: ChannelComponent },
          { path: 'videos', component: ChannelVideosComponent },
          { path: 'playlists', component: ChannelPlaylistsComponent }
        ]
      },

      // 創作者工作台（包含您之前的 Guard 邏輯）
      {
        path: 'videocreator',
        component: VideocreatorLayoutComponent,
        canActivate: [videoGuard],
        canActivateChild: [videoGuard],
        children: [
          { path: '', redirectTo: 'home', pathMatch: 'full' },
          { path: 'home', component: VideocreatorHomeComponent },
          { path: 'videos', component: VideocreatorVideomanageComponent },
          { path: 'upload', component: VideocreatorUploadComponent }
        ]
      }
    ]
  },

  // 萬用字元：處理所有找不到的路徑，跳轉回首頁
  { path: '**', redirectTo: 'home' }
];
