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
    path: 'social',
    loadComponent: () => import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./social/social.component').then(m => m.SocialComponent),
        children: [
          { path: '', redirectTo: 'index', pathMatch: 'full' },
          {
            path: 'index',
            loadComponent: () => import('./social/social-index/social-index.component').then(m => m.SocialIndexComponent)
          },
          {
            path: 'friends',
            loadComponent: () => import('./social/social-friends/social-friends.component').then(m => m.SocialFriendsComponent)
          },
        ]
      }
    ]
  },

];
