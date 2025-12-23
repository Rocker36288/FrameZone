import { Component } from '@angular/core';
import { SocialPostsSubmitComponent } from "../social-posts-submit/social-posts-submit.component";
import { SocialPostsComponent } from "../social-posts/social-posts.component";
import { SocialPostsListComponent } from "../social-posts-list/social-posts-list.component";

@Component({
  selector: 'app-social-index',
  imports: [SocialPostsSubmitComponent, SocialPostsComponent, SocialPostsListComponent],
  templateUrl: './social-index.component.html',
  styleUrl: './social-index.component.css'
})
export class SocialIndexComponent {

}
