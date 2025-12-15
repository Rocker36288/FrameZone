import { Routes } from '@angular/router';
import { VideosShellComponent } from './components/videos-shell.component';
import { VideoMainComponent } from './components/player/video-main/video-main.component';
import { ChannelLayoutComponent } from './components/channel/channel-layout/channel-layout.component';
import { ChannelVideosComponent } from './components/channel/channel-videos/channel-videos.component';
import { ChannelComponent } from './components/channel/channel-home/channel-home.component';
import { ChannelPlaylistsComponent } from './components/channel/channel-playlists/channel-playlists.component';
import { VideocreatorHomeComponent } from './components/creatorworkshop/videocreator-home/videocreator-home.component';
import { VideocreatorSidebarComponent } from './components/creatorworkshop/videocreator-sidebar/videocreator-sidebar.component';
import { VideocreatorVideomanageComponent } from './components/creatorworkshop/videocreator-videomanage/videocreator-videomanage.component';
import { VideocreatorUploadComponent } from './components/creatorworkshop/videocreator-upload/videocreator-upload.component';

export const VIDEO_ROUTES: Routes = [
  {
    path: '',
    component: VideosShellComponent,
  },
  {
    path: 'watch',
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
