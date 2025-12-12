import { Routes } from '@angular/router';
import { VideosShellComponent } from './components/videos-shell.component';
import { VideoMainComponent } from './components/player/video-main/video-main.component';

export const VIDEO_ROUTES: Routes = [
  {
    path: '',
    component: VideosShellComponent,
  },
  {
    path: 'watch',
    component: VideoMainComponent,
  },
]
