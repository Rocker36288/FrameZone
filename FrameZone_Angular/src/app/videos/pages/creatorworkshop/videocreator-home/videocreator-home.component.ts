import { Component } from '@angular/core';
import { VideocreatorSidebarComponent } from "../videocreator-sidebar/videocreator-sidebar.component";
import { VideocreatorEditvideoComponent } from "../videocreator-editvideo/videocreator-editvideo.component";

@Component({
  selector: 'app-videocreator-home',
  imports: [VideocreatorSidebarComponent, VideocreatorEditvideoComponent],
  templateUrl: './videocreator-home.component.html',
  styleUrl: './videocreator-home.component.css'
})
export class VideocreatorHomeComponent {

}
