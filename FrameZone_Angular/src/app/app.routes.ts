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
    path: 'photographer-bookinghome',
    loadComponent: () => import('./PhotographerBooking/photographer-bookinghome/photographer-bookinghome.component').then(m => m.PhotographerBookinghomeComponent)

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
      },
      {
        path: 'products',
        loadComponent: () => import('./shopping/shopping-products/shopping-products.component').then(m => m.ShoppingProductsComponent)
      },
      {
        path: 'product-detail/:productId',
        loadComponent: () => import('./shopping/shopping-product-detail/shopping-product-detail.component').then(m => m.ShoppingProductDetailComponent)
      },
      {
        path: 'sellershop/:sellerAccount',
        loadComponent: () => import('./shopping/shopping-sellershop/shopping-sellershop.component').then(m => m.ShoppingSellershopComponent)
      },
      {
        path: 'buyer-center',
        loadComponent: () => import('./shopping/shopping-buyer-center/shopping-buyer-center.component').then(m => m.ShoppingBuyerCenterComponent)
      },
      {
        path: 'reviews',
        loadComponent: () => import('./shopping/shopping-reviews/shopping-reviews.component').then(m => m.ShoppingReviewsComponent)
      },
      {
        path: 'chats',
        loadComponent: () => import('./shopping/shopping-chats/shopping-chats.component').then(m => m.ShoppingChatsComponent)
      }
    ]
  },
  {
    path: 'shoppingcart',
    loadComponent: () => import('./shopping/shoppingcart/shoppingcart.component').then(m => m.ShoppingcartComponent)
  },
  {
    path: 'checkout',
    loadComponent: () => import('./shopping/shopping-checkout/shopping-checkout.component').then(m => m.ShoppingCheckoutComponent)
  },
  {
    path: 'order-success',
    loadComponent: () => import('./shopping/shopping-order-success/shopping-order-success.component').then(m => m.ShoppingOrderSuccessComponent)
  },
  {
    path: 'help-center',
    loadComponent: () => import('./shopping/shopping-help-center/shopping-help-center.component').then(m => m.ShoppingHelpCenterComponent)
  },
  {
    path: 'seller',
    loadComponent: () => import('./shopping/seller/seller-layout/seller-layout.component').then(m => m.SellerLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: 'seller-management',
        pathMatch: 'full'
      },
      {
        path: 'myshop-settings',
        loadComponent: () => import('./shopping/seller/myshop-settings/myshop-settings.component').then(m => m.MyshopSettingsComponent)
      },
      {
        path: 'category-settings',
        loadComponent: () => import('./shopping/seller/category-settings/category-settings.component').then(m => m.CategorySettingsComponent)
      },
      {
        path: 'products/:status',
        loadComponent: () => import('./shopping/seller/product-management/product-management.component').then(m => m.ProductManagementComponent)
      },
      {
        path: 'products',
        redirectTo: 'products/all',
        pathMatch: 'full'
      },
      {
        path: 'orders/:status',
        loadComponent: () => import('./shopping/seller/order-management/order-management.component').then(m => m.OrderManagementComponent)
      },
      {
        path: 'orders',
        redirectTo: 'orders/all',
        pathMatch: 'full'
        // 建議保留一個不帶參數的預設路徑，避免直接輸入 /seller/orders 報錯
      },
      {
        path: 'reviews',
        loadComponent: () => import('./shopping/seller/review-management/review-management.component').then(m => m.ReviewManagementComponent)
      },
      {
        path: 'wallet',
        loadComponent: () => import('./shopping/seller/wallet-management/wallet-management.component').then(m => m.WalletManagementComponent)
      },
      {
        path: 'bank-account',
        loadComponent: () => import('./shopping/seller/bank-account-management/bank-account-management.component').then(m => m.BankAccountManagementComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./shopping/seller/settings/settings.component').then(m => m.SettingsComponent),
        children: [
          {
            path: '',
            redirectTo: 'coupons',
            pathMatch: 'full'
          },
          {
            path: 'coupons',
            loadComponent: () => import('./shopping/seller/settings/coupon-settings/coupon-settings.component').then(m => m.CouponSettingsComponent)
          },
          {
            path: 'shipping',
            loadComponent: () => import('./shopping/seller/settings/shipping-settings/shipping-settings.component').then(m => m.ShippingSettingsComponent)
          },
          {
            path: 'payment',
            loadComponent: () => import('./shopping/seller/settings/payment-settings/payment-settings.component').then(m => m.PaymentSettingsComponent)
          },
          {
            path: 'chat',
            loadComponent: () => import('./shopping/seller/settings/chat-settings/chat-settings.component').then(m => m.ChatSettingsComponent)
          }
        ]
      }
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
