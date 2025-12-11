import { Component } from '@angular/core';
import { HeaderComponent } from "../../shared/components/header/header.component";
import { VideoMainComponent } from "./player/video-main/video-main.component";

@Component({
  selector: 'app-videos-shell',
  imports: [HeaderComponent, VideoMainComponent],
  templateUrl: './videos-shell.component.html',
  styleUrl: './videos-shell.component.css'
})
export class VideosShellComponent {


}
