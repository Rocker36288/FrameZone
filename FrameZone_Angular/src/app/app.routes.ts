import { Title } from '@angular/platform-browser';
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./layouts/auth-layout/auth-layout.component').then(m => m.AuthLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent),
        title: 'FrameZone - 登入'
      }
    ]
  },
  {
    path: 'register',
    loadComponent: () => import('./layouts/auth-layout/auth-layout.component').then(m => m.AuthLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/auth/register/register.component').then(m => m.RegisterComponent),
        title: 'FrameZone - 註冊'
      }
    ]
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./layouts/auth-layout/auth-layout.component').then(m => m.AuthLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent),
        title: 'FrameZone - 忘記密碼'
      }
    ]
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./layouts/auth-layout/auth-layout.component').then(m => m.AuthLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent),
        title: 'FrameZone - 重設密碼'
      }
    ]
  },
  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) },
    ]
  },
  {
    path: '',
    loadComponent: () => import('./layouts/photo-layout/photo-layout.component').then(m => m.PhotoLayoutComponent),
    children: [
      { path: 'photo-home', loadComponent: () => import('./pages/photo/photo-home/photo-home.component').then(m => m.PhotoHomeComponent) },
      { path: 'photo-classify', loadComponent: () => import('./pages/photo/photo-classify/photo-classify.component').then(m => m.PhotoClassifyComponent) },
      { path: 'photo-myphoto', loadComponent: () => import('./pages/photo/photo-myphoto/photo-myphoto.component').then(m => m.PhotoMyphotoComponent) },
      { path: 'photo-price', loadComponent: () => import('./pages/photo/photo-price/photo-price.component').then(m => m.PhotoPriceComponent) },
      { path: 'photo-about', loadComponent: () => import('./pages/photo/photo-about/photo-about.component').then(m => m.PhotoAboutComponent) }
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


  {
    path: 'videos',
    loadChildren: () => import('./videos/videos.routes').then(m => m.VIDEO_ROUTES)
  },
];
