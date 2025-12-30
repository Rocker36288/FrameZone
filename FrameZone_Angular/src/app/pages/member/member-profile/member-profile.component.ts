import { AuthService } from './../../../core/services/auth.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import heic2any from 'heic2any';

// 從 Models 導入
import {
  UserProfileDto,
  UpdateUserProfileDto,
  GetProfileResponseDto,
  UpdateProfileResponseDto,
  GenderOption
} from '../../../core/models/member.models';

// 從 Services 導入
import { MemberService } from '../../../core/services/member.service';

// 從 Constants 導入
import {
  MEMBER_FIELD_LIMITS,
  MEMBER_IMAGE_LIMITS,
  GENDER_OPTIONS,
  isValidAvatarFile,
  isValidCoverImageFile,
  isValidUrl,
  isValidPhone,
  getImagePreviewUrl,
  isHeicFormat
} from '../../../shared/constants/member.constants';
import { CommonModule } from '@angular/common';

/**
 * 會員個人資料編輯元件
 */
@Component({
  selector: 'app-member-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './member-profile.component.html',
  styleUrls: ['./member-profile.component.css']
})
export class MemberProfileComponent implements OnInit {

  // 儲存狀態
  isSaving: boolean = false;

  // 錯誤訊息
  loadError: string | null = null;
  saveError: string | null = null;
  saveSuccess: string | null = null;

  // 使用者資料
  profile: UserProfileDto | null = null;

  // 表單資料
  formData: UpdateUserProfileDto = {
    phone: null,
    displayName: null,
    bio: null,
    website: null,
    location: null,
    realName: null,
    gender: null,
    birthDate: null,
    fullAddress: null,
    country: null,
    city: null,
    postalCode: null,
    avatarFile: null,
    coverImageFile: null,
    removeAvatar: false,
    removeCoverImage: false
  };

  // 圖片預覽
  avatarPreview: string | null = null;
  coverImagePreview: string | null = null;

  // HEIC 檔案標記（轉換失敗時使用）
  avatarIsHeic: boolean = false;
  coverImageIsHeic: boolean = false;
  avatarFileName: string = '';
  coverImageFileName: string = '';

  // 常數
  readonly fieldLimits = MEMBER_FIELD_LIMITS;
  readonly imageLimits = MEMBER_IMAGE_LIMITS;
  readonly genderOptions: readonly GenderOption[] = GENDER_OPTIONS;

  constructor(
    private memberService: MemberService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadProfile();
  }

  /**
   * 載入使用者個人資料
   */
  loadProfile(): void {
    this.loadError = null;

    this.memberService.getProfile().subscribe({
      next: (response: GetProfileResponseDto) => {
        this.profile = response.data;
        this.initializeFormData(response.data);
        console.log('✅ 個人資料載入成功', response.data);
      },
      error: (error) => {
        console.error('❌ 載入個人資料失敗', error);
        this.loadError = error.error?.message || '載入個人資料失敗，請稍後再試';
      }
    });
  }

  /**
   * 初始化表單資料
   */
  private initializeFormData(profile: UserProfileDto): void {
    this.formData = {
      phone: profile.phone,
      displayName: profile.displayName,
      bio: profile.bio,
      website: profile.website,
      location: profile.location,
      realName: profile.realName,
      gender: profile.gender,
      birthDate: profile.birthDate,
      fullAddress: profile.fullAddress,
      country: profile.country,
      city: profile.city,
      postalCode: profile.postalCode,
      avatarFile: null,
      coverImageFile: null,
      removeAvatar: false,
      removeCoverImage: false
    };

    this.avatarPreview = profile.avatar;
    this.coverImagePreview = profile.coverImage;

    console.log('📋 表單資料已初始化', {
      hasAvatar: !!this.avatarPreview,
      hasCoverImage: !!this.coverImagePreview
    });
  }

  /**
   * 轉換 HEIC 檔案為 JPEG (用於預覽)
   * @returns 轉換後的 File，如果轉換失敗則返回 null
   */
  private async convertHeicToJpeg(file: File): Promise<File | null> {
    try {
      console.log('🔄 開始轉換 HEIC 到 JPEG...');

      const convertedBlob = await heic2any({
        blob: file,
        toType: 'image/jpeg',
        quality: 0.8
      });

      // heic2any 可能返回 Blob 或 Blob[]
      const blob = Array.isArray(convertedBlob) ? convertedBlob[0] : convertedBlob;

      // 創建新的 File 對象
      const convertedFile = new File(
        [blob],
        file.name.replace(/\.heic$/i, '.jpg'),
        { type: 'image/jpeg' }
      );

      console.log('✅ HEIC 轉換成功', {
        originalSize: (file.size / 1024).toFixed(2) + ' KB',
        convertedSize: (convertedFile.size / 1024).toFixed(2) + ' KB'
      });

      return convertedFile;
    } catch (error: any) {
      console.error('❌ HEIC 轉換失敗', error);
      console.error('錯誤詳情:', {
        code: error?.code,
        message: error?.message,
        name: error?.name
      });

      // 返回 null 表示轉換失敗，調用者會顯示佔位符
      return null;
    }
  }

  /**
   * 處理頭像檔案選擇
   */
  async onAvatarFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      console.log('❌ 沒有選擇檔案');
      return;
    }

    const file = input.files[0];
    console.log('📤 選擇頭像檔案:', {
      name: file.name,
      type: file.type,
      size: file.size,
      sizeInMB: (file.size / (1024 * 1024)).toFixed(2) + ' MB',
      isHeic: isHeicFormat(file)
    });

    // 驗證檔案
    const validation = isValidAvatarFile(file);
    if (!validation.valid) {
      console.error('❌ 檔案驗證失敗:', validation.error);
      alert(validation.error);
      input.value = '';
      return;
    }

    console.log('✅ 檔案驗證通過，開始處理...');

    try {
      const isHeic = isHeicFormat(file);

      if (isHeic) {
        console.log('🖼️ 檢測到 HEIC 格式，嘗試轉換...');

        // 嘗試轉換 HEIC
        const convertedFile = await this.convertHeicToJpeg(file);

        if (convertedFile) {
          // 轉換成功：顯示預覽
          const previewUrl = await getImagePreviewUrl(convertedFile);
          console.log('✅ 預覽 URL 生成成功');

          this.avatarPreview = previewUrl;
          this.avatarIsHeic = false;
          this.avatarFileName = '';
        } else {
          // 轉換失敗：顯示佔位符
          console.log('⚠️ HEIC 轉換失敗，使用佔位符');

          this.avatarPreview = null;
          this.avatarIsHeic = true;
          this.avatarFileName = file.name;
        }
      } else {
        // 一般格式：直接顯示預覽
        const previewUrl = await getImagePreviewUrl(file);
        console.log('✅ 預覽 URL 生成成功');

        this.avatarPreview = previewUrl;
        this.avatarIsHeic = false;
        this.avatarFileName = '';
      }

      // 保存原始檔案（不論是否轉換成功）
      this.formData.avatarFile = file;
      this.formData.removeAvatar = false;

      console.log('✅ 頭像設定完成', {
        originalFormat: file.name.split('.').pop(),
        hasPreview: !!this.avatarPreview,
        showPlaceholder: this.avatarIsHeic
      });
    } catch (error) {
      console.error('❌ 處理圖片失敗', error);
      alert('處理圖片失敗，請重試。\n錯誤: ' + error);
      input.value = '';
    }
  }

  /**
   * 處理封面圖片檔案選擇
   */
  async onCoverImageFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      console.log('❌ 沒有選擇檔案');
      return;
    }

    const file = input.files[0];
    console.log('📤 選擇封面圖片檔案:', {
      name: file.name,
      type: file.type,
      size: file.size,
      sizeInMB: (file.size / (1024 * 1024)).toFixed(2) + ' MB',
      isHeic: isHeicFormat(file)
    });

    // 驗證檔案
    const validation = isValidCoverImageFile(file);
    if (!validation.valid) {
      console.error('❌ 檔案驗證失敗:', validation.error);
      alert(validation.error);
      input.value = '';
      return;
    }

    console.log('✅ 檔案驗證通過，開始處理...');

    try {
      const isHeic = isHeicFormat(file);

      if (isHeic) {
        console.log('🖼️ 檢測到 HEIC 格式，嘗試轉換...');

        // 嘗試轉換 HEIC
        const convertedFile = await this.convertHeicToJpeg(file);

        if (convertedFile) {
          // 轉換成功：顯示預覽
          const previewUrl = await getImagePreviewUrl(convertedFile);
          console.log('✅ 預覽 URL 生成成功');

          this.coverImagePreview = previewUrl;
          this.coverImageIsHeic = false;
          this.coverImageFileName = '';
        } else {
          // 轉換失敗：顯示佔位符
          console.log('⚠️ HEIC 轉換失敗，使用佔位符');

          this.coverImagePreview = null;
          this.coverImageIsHeic = true;
          this.coverImageFileName = file.name;
        }
      } else {
        // 一般格式：直接顯示預覽
        const previewUrl = await getImagePreviewUrl(file);
        console.log('✅ 預覽 URL 生成成功');

        this.coverImagePreview = previewUrl;
        this.coverImageIsHeic = false;
        this.coverImageFileName = '';
      }

      // 保存原始檔案（不論是否轉換成功）
      this.formData.coverImageFile = file;
      this.formData.removeCoverImage = false;

      console.log('✅ 封面圖片設定完成', {
        originalFormat: file.name.split('.').pop(),
        hasPreview: !!this.coverImagePreview,
        showPlaceholder: this.coverImageIsHeic
      });
    } catch (error) {
      console.error('❌ 處理圖片失敗', error);
      alert('處理圖片失敗，請重試。\n錯誤: ' + error);
      input.value = '';
    }
  }

  /**
   * 移除頭像
   */
  removeAvatar(): void {
    this.avatarPreview = null;
    this.formData.avatarFile = null;
    this.formData.removeAvatar = true;
    console.log('🗑️ 標記移除頭像');
  }

  /**
   * 移除封面圖片
   */
  removeCoverImage(): void {
    this.coverImagePreview = null;
    this.formData.coverImageFile = null;
    this.formData.removeCoverImage = true;
    console.log('🗑️ 標記移除封面圖片');
  }

  /**
   * 驗證表單（只檢查明顯錯誤,提升使用者體驗）
   * 真正的驗證由後端處理
   */
  validateForm(): { valid: boolean; errors: string[] } {
    const errors: string[] = [];

    // 1. 網站 URL 格式（明顯錯誤）
    if (this.formData.website &&
      this.formData.website.trim() !== '' &&
      !isValidUrl(this.formData.website)) {
      errors.push('網站格式不正確（需以 http:// 或 https:// 開頭）');
    }

    // 2. 電話格式（明顯錯誤：包含非數字、空格、+、-、()以外的字符）
    if (this.formData.phone &&
      this.formData.phone.trim() !== '' &&
      !isValidPhone(this.formData.phone)) {
      errors.push('電話格式不正確');
    }

    // 注意：欄位長度、必填欄位、性別選項等驗證都由後端處理
    // 前端只提示明顯的格式錯誤，提升使用者體驗

    return {
      valid: errors.length === 0,
      errors
    };
  }

  /**
   * 提交表單
   */
  onSubmit(): void {
    console.log('📝 提交表單', {
      hasAvatarFile: !!this.formData.avatarFile,
      hasCoverImageFile: !!this.formData.coverImageFile,
      avatarFileType: this.formData.avatarFile?.name.split('.').pop(),
      coverImageFileType: this.formData.coverImageFile?.name.split('.').pop(),
      removeAvatar: this.formData.removeAvatar,
      removeCoverImage: this.formData.removeCoverImage
    });

    // 前端基本驗證
    const validation = this.validateForm();
    if (!validation.valid) {
      this.saveError = validation.errors.join('\n');
      console.error('❌ 表單驗證失敗', validation.errors);
      return;
    }

    this.isSaving = true;
    this.saveError = null;
    this.saveSuccess = null;

    this.memberService.updateProfile(this.formData).subscribe({
      next: (response: UpdateProfileResponseDto) => {
        console.log('✅ 更新成功', response);
        this.saveSuccess = response.message || '個人資料更新成功';
        this.isSaving = false;

        // 重新載入最新資料（包含後端處理後的圖片 URL）
        this.loadProfile();

        if (response.data) {
          this.authService.updateUserSession({
            displayName: response.data.displayName as string | undefined,
            avatar: response.data.avatar as string | undefined,
          });
          console.log('🔄 已同步更新用戶 Session');
        }

        // 3 秒後自動隱藏成功訊息
        setTimeout(() => {
          this.saveSuccess = null;
        }, 3000);

        // 平滑滾動到頂部
        window.scrollTo({
          top: 0,
          behavior: 'smooth'
        });
      },
      error: (error) => {
        console.error('❌ 更新個人資料失敗', error);

        // 處理後端驗證錯誤
        if (error.error?.message) {
          this.saveError = error.error.message;
        } else if (error.error?.errors) {
          // 如果後端返回驗證錯誤列表
          const errorMessages = Object.values(error.error.errors).flat();
          this.saveError = errorMessages.join('\n');
        } else {
          this.saveError = '更新個人資料失敗，請稍後再試';
        }

        this.isSaving = false;

        // 平滑滾動到頂部顯示錯誤
        window.scrollTo({
          top: 0,
          behavior: 'smooth'
        });
      }
    });
  }

  /**
   * 取消編輯
   */
  onCancel(): void {
    if (this.profile) {
      this.initializeFormData(this.profile);
    }
    this.saveError = null;
    this.saveSuccess = null;
    console.log('↩️ 取消編輯，恢復原始資料');

    // 平滑滾動到頂部
    window.scrollTo({
      top: 0,
      behavior: 'smooth'
    });
  }

  /**
   * 計算剩餘字元數
   */
  getRemainingCharacters(text: string | null, maxLength: number): number {
    const length = text ? text.length : 0;
    return maxLength - length;
  }
}
