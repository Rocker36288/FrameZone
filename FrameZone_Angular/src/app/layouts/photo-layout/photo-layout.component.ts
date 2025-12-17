import { Component } from '@angular/core';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { RouterModule, RouterOutlet } from "@angular/router";
import { UserMenuComponent } from "../../shared/components/user-menu/user-menu.component";
import { PhotoFooterComponent } from "../../shared/components/photo-footer/photo-footer.component";

@Component({
  selector: 'app-photo-layout',
  imports: [FooterComponent, RouterOutlet, RouterModule, UserMenuComponent, PhotoFooterComponent],
  templateUrl: './photo-layout.component.html',
  styleUrl: './photo-layout.component.css'
})
export class PhotoLayoutComponent {

}
