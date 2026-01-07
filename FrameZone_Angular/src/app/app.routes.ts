import { Title } from '@angular/platform-browser';
import { Routes } from '@angular/router';
import { videoGuard } from './videos/guard/video.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./layouts/auth-layout/auth-layout.component').then(
        (m) => m.AuthLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/login/login.component').then((m) => m.LoginComponent),
        title: 'FrameZone - ?餃',
      },
    ],
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./layouts/auth-layout/auth-layout.component').then(
        (m) => m.AuthLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/auth/register/register.component').then(
            (m) => m.RegisterComponent
          ),
        title: 'FrameZone - 閮餃?',
      },
    ],
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./layouts/auth-layout/auth-layout.component').then(
        (m) => m.AuthLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/auth/forgot-password/forgot-password.component').then(
            (m) => m.ForgotPasswordComponent
          ),
        title: 'FrameZone - 敹?撖Ⅳ',
      },
    ],
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./layouts/auth-layout/auth-layout.component').then(
        (m) => m.AuthLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/auth/reset-password/reset-password.component').then(
            (m) => m.ResetPasswordComponent
          ),
        title: 'FrameZone - ?身撖Ⅳ',
      },
    ],
  },
  {
    path: 'photographer-bookinghome',
    loadComponent: () =>
      import(
        './PhotographerBooking/photographer-bookinghome/photographer-bookinghome.component'
      ).then((m) => m.PhotographerBookinghomeComponent),
  },
  {
    path: 'photographerbooking-page-search',
    loadComponent: () =>
      import(
        './PhotographerBooking/photographerbooking-page-search/photographerbooking-page-search.component'
      ).then((m) => m.PhotographerbookingPageSearchComponent),
  },
  {
    path: 'photographer-detail',
    loadComponent: () =>
      import(
        './PhotographerBooking/photographer-detail/photographer-detail.component'
      ).then((m) => m.PhotographerDetailComponent),
  },
  {
    path: '',
    loadComponent: () =>
      import('./layouts/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      {
        path: 'home',
        loadComponent: () =>
          import('./pages/home/home.component').then((m) => m.HomeComponent),
      },
    ],
  },
  {
    path: 'member',
    loadComponent: () =>
      import('./layouts/member-layout/member-layout.component').then(
        (m) => m.MemberLayoutComponent
      ),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import(
            './pages/member/member-dashboard/member-dashboard.component'
          ).then((m) => m.MemberDashboardComponent),
        title: 'FrameZone - ?銝剖?',
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./pages/member/member-profile/member-profile.component').then(
            (m) => m.MemberProfileComponent
          ),
        title: 'FrameZone - ?犖鞈?',
      },
      {
        path: 'security',
        loadComponent: () =>
          import(
            './pages/member/member-security/member-security.component'
          ).then((m) => m.MemberSecurityComponent),
        title: 'FrameZone - 撣唾?摰',
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import(
            './pages/member/member-notifications/member-notifications.component'
          ).then((m) => m.MemberNotificationsComponent),
        title: 'FrameZone - ?閮剖?',
      },
      {
        path: 'privacy',
        loadComponent: () =>
          import('./pages/member/member-privacy/member-privacy.component').then(
            (m) => m.MemberPrivacyComponent
          ),
        title: 'FrameZone - ?梁?閮剖?',
      },
      {
        path: 'logs',
        loadComponent: () =>
          import('./pages/member/member-logs/member-logs.component').then(
            (m) => m.MemberLogsComponent
          ),
        title: 'FrameZone - ??閮?',
      },
    ],
  },
  {
    path: 'shopping',
    loadComponent: () =>
      import('./shopping/shopping-header/shopping-header.component').then(
        (m) => m.ShoppingHeaderComponent
      ),
    children: [
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full',
      },
      {
        path: 'home',
        loadComponent: () =>
          import('./shopping/shoppinghome/shoppinghome.component').then(
            (m) => m.ShoppinghomeComponent
          ),
      },
      {
        path: 'products',
        loadComponent: () =>
          import(
            './shopping/shopping-products/shopping-products.component'
          ).then((m) => m.ShoppingProductsComponent),
      },
      {
        path: 'product-detail/:productId',
        loadComponent: () =>
          import(
            './shopping/shopping-product-detail/shopping-product-detail.component'
          ).then((m) => m.ShoppingProductDetailComponent),
      },
      {
        path: 'sellershop/:sellerAccount',
        loadComponent: () =>
          import(
            './shopping/shopping-sellershop/shopping-sellershop.component'
          ).then((m) => m.ShoppingSellershopComponent),
      },
      {
        path: 'buyer-center',
        loadComponent: () =>
          import(
            './shopping/shopping-buyer-center/shopping-buyer-center.component'
          ).then((m) => m.ShoppingBuyerCenterComponent),
      },
      {
        path: 'reviews',
        loadComponent: () =>
          import('./shopping/shopping-reviews/shopping-reviews.component').then(
            (m) => m.ShoppingReviewsComponent
          ),
      },
      {
        path: 'chats',
        loadComponent: () =>
          import('./shopping/shopping-chats/shopping-chats.component').then(
            (m) => m.ShoppingChatsComponent
          ),
      },
    ],
  },
  {
    path: 'shoppingcart',
    loadComponent: () =>
      import('./shopping/shoppingcart/shoppingcart.component').then(
        (m) => m.ShoppingcartComponent
      ),
  },
  {
    path: 'checkout',
    loadComponent: () =>
      import('./shopping/shopping-checkout/shopping-checkout.component').then(
        (m) => m.ShoppingCheckoutComponent
      ),
  },
  {
    path: 'order-success',
    loadComponent: () =>
      import(
        './shopping/shopping-order-success/shopping-order-success.component'
      ).then((m) => m.ShoppingOrderSuccessComponent),
  },
  {
    path: 'help-center',
    loadComponent: () =>
      import(
        './shopping/shopping-help-center/shopping-help-center.component'
      ).then((m) => m.ShoppingHelpCenterComponent),
  },
  {
    path: 'seller',
    loadComponent: () =>
      import('./shopping/seller/seller-layout/seller-layout.component').then(
        (m) => m.SellerLayoutComponent
      ),
    children: [
      {
        path: '',
        redirectTo: 'seller-management',
        pathMatch: 'full',
      },
      {
        path: 'myshop-settings',
        loadComponent: () =>
          import(
            './shopping/seller/myshop-settings/myshop-settings.component'
          ).then((m) => m.MyshopSettingsComponent),
      },
      {
        path: 'category-settings',
        loadComponent: () =>
          import(
            './shopping/seller/category-settings/category-settings.component'
          ).then((m) => m.CategorySettingsComponent),
      },
      {
        path: 'products/:status',
        loadComponent: () =>
          import(
            './shopping/seller/product-management/product-management.component'
          ).then((m) => m.ProductManagementComponent),
      },
      {
        path: 'products',
        redirectTo: 'products/all',
        pathMatch: 'full',
      },
      {
        path: 'orders/:status',
        loadComponent: () =>
          import(
            './shopping/seller/order-management/order-management.component'
          ).then((m) => m.OrderManagementComponent),
      },
      {
        path: 'orders',
        redirectTo: 'orders/all',
        pathMatch: 'full',
        // 撱箄降靽?銝??撣嗅??貊??身頝臬?嚗??亥撓??/seller/orders ?梢
      },
      {
        path: 'reviews',
        loadComponent: () =>
          import(
            './shopping/seller/review-management/review-management.component'
          ).then((m) => m.ReviewManagementComponent),
      },
      {
        path: 'wallet',
        loadComponent: () =>
          import(
            './shopping/seller/wallet-management/wallet-management.component'
          ).then((m) => m.WalletManagementComponent),
      },
      {
        path: 'bank-account',
        loadComponent: () =>
          import(
            './shopping/seller/bank-account-management/bank-account-management.component'
          ).then((m) => m.BankAccountManagementComponent),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./shopping/seller/settings/settings.component').then(
            (m) => m.SettingsComponent
          ),
        children: [
          {
            path: '',
            redirectTo: 'coupons',
            pathMatch: 'full',
          },
          {
            path: 'coupons',
            loadComponent: () =>
              import(
                './shopping/seller/settings/coupon-settings/coupon-settings.component'
              ).then((m) => m.CouponSettingsComponent),
          },
          {
            path: 'shipping',
            loadComponent: () =>
              import(
                './shopping/seller/settings/shipping-settings/shipping-settings.component'
              ).then((m) => m.ShippingSettingsComponent),
          },
          {
            path: 'payment',
            loadComponent: () =>
              import(
                './shopping/seller/settings/payment-settings/payment-settings.component'
              ).then((m) => m.PaymentSettingsComponent),
          },
          {
            path: 'chat',
            loadComponent: () =>
              import(
                './shopping/seller/settings/chat-settings/chat-settings.component'
              ).then((m) => m.ChatSettingsComponent),
          },
        ],
      },
    ],
  },
  {
    path: '',
    loadComponent: () =>
      import('./layouts/photo-layout/photo-layout.component').then(
        (m) => m.PhotoLayoutComponent
      ),
    children: [
      {
        path: 'photo-home',
        loadComponent: () =>
          import('./pages/photo/photo-home/photo-home.component').then(
            (m) => m.PhotoHomeComponent
          ),
      },
      {
        path: 'photo-classify',
        loadComponent: () =>
          import('./pages/photo/photo-classify/photo-classify.component').then(
            (m) => m.PhotoClassifyComponent
          ),
      },
      {
        path: 'photo-myphoto',
        loadComponent: () =>
          import('./pages/photo/photo-myphoto/photo-myphoto.component').then(
            (m) => m.PhotoMyphotoComponent
          ),
      },
      {
        path: 'photo-price',
        loadComponent: () =>
          import('./pages/photo/photo-price/photo-price.component').then(
            (m) => m.PhotoPriceComponent
          ),
      },
      {
        path: 'photo-about',
        loadComponent: () =>
          import('./pages/photo/photo-about/photo-about.component').then(
            (m) => m.PhotoAboutComponent
          ),
      },
    ],
  },
  {
    path: 'social',
    // canActivateChild: [videoGuard], 銋????餃
    loadComponent: () =>
      import('./layouts/social-layout/social-layout.component').then(
        (m) => m.SocialLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./social/social.component').then((m) => m.SocialComponent),
        children: [
          { path: '', redirectTo: 'index', pathMatch: 'full' },
          {
            path: 'index',
            loadComponent: () =>
              import('./social/social-index/social-index.component').then(
                (m) => m.SocialIndexComponent
              ),
          },
          {
            path: 'profile',
            loadComponent: () =>
              import('./social/social-profile/social-profile.component').then(
                (m) => m.SocialProfileComponent
              ),
          },
          {
            path: 'profile/:userId',
            loadComponent: () =>
              import('./social/social-profile/social-profile.component').then(
                (m) => m.SocialProfileComponent
              ),
          },
          {
            path: 'following',
            loadComponent: () =>
              import('./social/social-posts-list/social-posts-list.component').then(
                (m) => m.SocialPostsListComponent
              ),
          },
          {
            path: 'recent',
            loadComponent: () =>
              import('./social/social-posts-list/social-posts-list.component').then(
                (m) => m.SocialPostsListComponent
              ),
          },
          {
            path: 'liked',
            loadComponent: () =>
              import('./social/social-posts-list/social-posts-list.component').then(
                (m) => m.SocialPostsListComponent
              ),
          },
          {
            path: 'commented',
            loadComponent: () =>
              import('./social/social-posts-list/social-posts-list.component').then(
                (m) => m.SocialPostsListComponent
              ),
          },
          {
            path: 'shared',
            loadComponent: () =>
              import('./social/social-posts-list/social-posts-list.component').then(
                (m) => m.SocialPostsListComponent
              ),
          },
        ],
      },
    ],
  },
  {
    path: 'videos',
    loadChildren: () =>
      import('./videos/videos.routes').then((m) => m.VIDEO_ROUTES),
  },
];


