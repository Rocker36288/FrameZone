import { Component } from '@angular/core';
import { SocialPostsComponent } from "../social-posts/social-posts.component";
import { SocialShowpostsComponent } from "../social-showposts/social-showposts.component";
import { SocialPostlistComponent } from "../social-postlist/social-postlist.component";

@Component({
  selector: 'app-social-index',
  imports: [SocialPostsComponent, SocialShowpostsComponent, SocialPostlistComponent],
  templateUrl: './social-index.component.html',
  styleUrl: './social-index.component.css'
})
export class SocialIndexComponent {

}
