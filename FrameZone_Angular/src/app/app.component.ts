import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ShoppingcartComponent } from "./shopping/shoppingcart/shoppingcart.component";
import { ShoppingHeaderComponent } from "./shopping/shopping-header/shopping-header.component";
import { ShoppingCheckoutComponent } from "./shopping/shopping-checkout/shopping-checkout.component";
import { ShoppinghomeComponent } from "./shopping/shoppinghome/shoppinghome.component";
import { ShoppingProductDetailComponent } from "./shopping/shopping-product-detail/shopping-product-detail.component";
import { ShoppingSellershopComponent } from "./shopping/shopping-sellershop/shopping-sellershop.component";
import { ShoppingBuyerCenterComponent } from "./shopping/shopping-buyer-center/shopping-buyer-center.component";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ShoppingcartComponent, ShoppingHeaderComponent, ShoppingCheckoutComponent, ShoppinghomeComponent, ShoppingProductDetailComponent, ShoppingSellershopComponent, ShoppingBuyerCenterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'FrameZone_Angular';
}
