import { Component, ViewEncapsulation } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HomethemeComponent } from '../hometheme/hometheme.component';
import { AccountComponent } from '../account/account.component';

@Component({
  selector: 'app-navbar',
  imports: [RouterModule, HomethemeComponent, AccountComponent],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
  encapsulation: ViewEncapsulation.None
})
export class NavbarComponent {

}
