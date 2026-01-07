import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SellerNavbarComponent } from './seller-navbar/seller-navbar.component';
import { SellerSidebarComponent } from './seller-sidebar/seller-sidebar.component';

@Component({
  selector: 'app-seller-layout',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule, SellerNavbarComponent, SellerSidebarComponent],
  templateUrl: './seller-layout.component.html',
  styleUrl: './seller-layout.component.css'
})
export class SellerLayoutComponent {
  isSidebarCollapsed = false;

  toggleSidebar() {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}
