import { Routes } from '@angular/router';
import { VideosShellComponent } from './pages/videos-shell.component';
import { VideoMainComponent } from './pages/player/video-main/video-main.component';
import { ChannelLayoutComponent } from './pages/channel/channel-layout/channel-layout.component';
import { ChannelVideosComponent } from './pages/channel/channel-videos/channel-videos.component';
import { ChannelComponent } from './pages/channel/channel-home/channel-home.component';
import { ChannelPlaylistsComponent } from './pages/channel/channel-playlists/channel-playlists.component';
import { VideocreatorHomeComponent } from './pages/creatorworkshop/videocreator-home/videocreator-home.component';
import { VideocreatorSidebarComponent } from './pages/creatorworkshop/videocreator-sidebar/videocreator-sidebar.component';
import { VideocreatorVideomanageComponent } from './pages/creatorworkshop/videocreator-videomanage/videocreator-videomanage.component';
import { VideocreatorUploadComponent } from './pages/creatorworkshop/videocreator-upload/videocreator-upload.component';

export const VIDEO_ROUTES: Routes = [
  {
    path: '',
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
    path: 'videocreator/:id',
    component: VideocreatorSidebarComponent,
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: VideocreatorHomeComponent },
      { path: 'videos', component: VideocreatorVideomanageComponent },
      { path: 'upload', component: VideocreatorUploadComponent }
    ]
  }
]
