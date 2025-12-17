import { Component } from '@angular/core';
import { HeaderComponent } from '../../shared/components/header/header.component';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-social-layout',
  imports: [HeaderComponent, RouterModule],
  templateUrl: './social-layout.component.html',
  styleUrl: './social-layout.component.css'
})
export class SocialLayoutComponent {

}
