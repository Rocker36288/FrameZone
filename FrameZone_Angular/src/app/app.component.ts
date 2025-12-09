import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ShoppingcartComponent } from "./shopping/shoppingcart/shoppingcart.component";
import { ShoppingHeaderComponent } from "./shopping/shopping-header/shopping-header.component";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ShoppingcartComponent, ShoppingHeaderComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'FrameZone_Angular';
}
