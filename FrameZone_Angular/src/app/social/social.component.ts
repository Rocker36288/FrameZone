import { Component, HostListener } from '@angular/core';
import { SocialMenuComponent } from "./social-menu/social-menu.component";
import { SocialFriendsComponent } from "./social-friends/social-friends.component";
import { SocialPostsComponent } from "./social-posts/social-posts.component";
import { SocialShowpostsComponent } from "./social-showposts/social-showposts.component";
import { NgClass } from '@angular/common';
import { RouterModule } from "@angular/router";

@Component({
  selector: 'app-social',
  imports: [SocialMenuComponent, SocialFriendsComponent, SocialPostsComponent, SocialShowpostsComponent, NgClass, RouterModule],
  templateUrl: './social.component.html',
  styleUrl: './social.component.css'
})
export class SocialComponent {

  // 偵測頁面捲動
  showBackToTop = false;
  @HostListener("window:scroll", [])
  onWindowScroll() {
    const scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
    this.showBackToTop = scrollTop > 100;
  }
  // 回到最上面
  backToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
