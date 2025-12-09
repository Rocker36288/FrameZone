import { Component } from '@angular/core';
import { SocialMenuComponent } from "./social-menu/social-menu.component";
import { SocialFriendsComponent } from "./social-friends/social-friends.component";
import { SocialPostsComponent } from "./social-posts/social-posts.component";
import { SocialShowpostsComponent } from "./social-showposts/social-showposts.component";

@Component({
  selector: 'app-social',
  imports: [SocialMenuComponent, SocialFriendsComponent, SocialPostsComponent, SocialShowpostsComponent],
  templateUrl: './social.component.html',
  styleUrl: './social.component.css'
})
export class SocialComponent {

}
