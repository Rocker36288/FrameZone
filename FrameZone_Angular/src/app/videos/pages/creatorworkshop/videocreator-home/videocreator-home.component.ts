import { Component } from '@angular/core';
import { VideocreatorEditvideoComponent } from "../videocreator-editvideo/videocreator-editvideo.component";
import { VideosSidebarComponent } from "../../../ui/videos-sidebar/videos-sidebar.component";
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-videocreator-home',
  imports: [VideocreatorEditvideoComponent, VideosSidebarComponent],
  templateUrl: './videocreator-home.component.html',
  styleUrl: './videocreator-home.component.css'
})
export class VideocreatorHomeComponent {
  userId: string | null = null;

  constructor(private authService: AuthService) { }

  ngOnInit() {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      // this.userId = currentUser.userId; // 或你 response 裡對應的欄位
    }
  }
}
