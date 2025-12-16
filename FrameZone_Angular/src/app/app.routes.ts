import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./layouts/auth-layout/auth-layout.component').then(m => m.AuthLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
      }
    ]
  },
  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) }
    ]
  },
  {
    path: 'shopping',
    loadComponent: () => import('./shopping/shopping-header/shopping-header.component').then(m => m.ShoppingHeaderComponent),
    children: [
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full'
      },
      {
        path: 'home',
        loadComponent: () => import('./shopping/shoppinghome/shoppinghome.component').then(m => m.ShoppinghomeComponent)
      }
    ]
  },
  {
    path: 'shoppingcart',
    loadComponent: () => import('./shopping/shoppingcart/shoppingcart.component').then(m => m.ShoppingcartComponent)
  }
];
