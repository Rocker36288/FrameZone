import { Component } from '@angular/core';
import { SocialPostsComponent } from "../social-posts/social-posts.component";
import { SocialShowpostsComponent } from "../social-showposts/social-showposts.component";

@Component({
  selector: 'app-social-index',
  imports: [SocialPostsComponent, SocialShowpostsComponent],
  templateUrl: './social-index.component.html',
  styleUrl: './social-index.component.css'
})
export class SocialIndexComponent {

}
