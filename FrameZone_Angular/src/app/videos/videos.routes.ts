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

export const VIDEO_ROUTES: Routes = [
  {
    path: 'home',
    component: VideosShellComponent,
  },
  {
    path: 'watch/:guid',
    component: VideoMainComponent,
  },
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
  },
  {
    path: '**',             // 匹配所有未定義的路徑
    redirectTo: 'home',     // 強制跳轉到 home
    pathMatch: 'full'
  }
]
