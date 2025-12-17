import { Component } from '@angular/core';
import { HeaderComponent } from "../../shared/components/header/header.component";
import { RouterModule } from "@angular/router";
import { FooterComponent } from "../../shared/components/footer/footer.component";

@Component({
  selector: 'app-main-layout',
  imports: [HeaderComponent, RouterModule, FooterComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent {

}
