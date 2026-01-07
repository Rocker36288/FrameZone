import { Component } from '@angular/core';
import { VideosSidebarComponent } from "../../../ui/videos-sidebar/videos-sidebar.component";
import { AuthService } from '../../../../core/services/auth.service';
import { VideocreatorHomeComponent } from "../videocreator-home/videocreator-home.component";
import { VideocreatorUploadComponent } from "../videocreator-upload/videocreator-upload.component";
import { RouterModule } from "@angular/router";

@Component({
  selector: 'app-videocreator-layout',
  imports: [VideosSidebarComponent, VideocreatorHomeComponent, VideocreatorUploadComponent, RouterModule],
  templateUrl: './videocreator-layout.component.html',
  styleUrl: './videocreator-layout.component.css'
})
export class VideocreatorLayoutComponent {
  userId: number | undefined;

  constructor(private authService: AuthService) { }

  ngOnInit() {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      this.userId = currentUser.userId; // 或你 response 裡對應的欄位
    }
  }
}
