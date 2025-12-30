import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MemberService } from '../../../core/services/member.service';
import {
  PrivacySettingDto,
  UpdatePrivacySettingDto,
  BatchUpdatePrivacySettingsDto
} from '../../../core/models/member.models';

/**
 * 隱私欄位配置
 */
interface PrivacyFieldConfig {
  fieldName: string;
  displayName: string;
  description: string;
  icon: string;
  category: 'basic' | 'private' | 'contact';
}

/**
 * 可見性選項
 */
interface VisibilityOption {
  value: string;
  label: string;
  description: string;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-member-privacy',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './member-privacy.component.html',
  styleUrl: './member-privacy.component.css'
})
export class MemberPrivacyComponent implements OnInit {
  // 狀態
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  // 隱私設定資料
  privacySettings: Map<string, string> = new Map();
  originalSettings: Map<string, string> = new Map();

  // 是否有變更
  hasChanges = false;

  // 隱私欄位配置
  privacyFields: PrivacyFieldConfig[] = [
    // 基本資訊
    {
      fieldName: 'Email',
      displayName: 'Email',
      description: '您的電子郵件地址',
      icon: '📧',
      category: 'basic'
    },
    {
      fieldName: 'Phone',
      displayName: '手機號碼',
      description: '您的聯絡電話',
      icon: '📱',
      category: 'contact'
    },
    {
      fieldName: 'Bio',
      displayName: '個人簡介',
      description: '關於您的簡短描述',
      icon: '📝',
      category: 'basic'
    },
    {
      fieldName: 'Website',
      displayName: '個人網站',
      description: '您的個人網站或部落格',
      icon: '🌐',
      category: 'basic'
    },
    {
      fieldName: 'Location',
      displayName: '所在地',
      description: '您目前的居住地',
      icon: '📍',
      category: 'basic'
    },

    // 私密資訊
    {
      fieldName: 'RealName',
      displayName: '真實姓名',
      description: '您的法定姓名',
      icon: '👤',
      category: 'private'
    },
    {
      fieldName: 'Gender',
      displayName: '性別',
      description: '您的性別',
      icon: '⚧',
      category: 'private'
    },
    {
      fieldName: 'BirthDate',
      displayName: '生日',
      description: '您的出生日期',
      icon: '🎂',
      category: 'private'
    },

    // 地址資訊
    {
      fieldName: 'FullAddress',
      displayName: '完整地址',
      description: '您的詳細住址',
      icon: '🏠',
      category: 'contact'
    },
    {
      fieldName: 'Country',
      displayName: '國家',
      description: '您所在的國家',
      icon: '🌍',
      category: 'contact'
    },
    {
      fieldName: 'City',
      displayName: '城市',
      description: '您所在的城市',
      icon: '🏙️',
      category: 'contact'
    },
    {
      fieldName: 'PostalCode',
      displayName: '郵遞區號',
      description: '您的郵遞區號',
      icon: '📮',
      category: 'contact'
    }
  ];

  // 可見性選項
  visibilityOptions: VisibilityOption[] = [
    {
      value: 'Public',
      label: '公開',
      description: '所有人都可以看到',
      icon: '🌐',
      color: 'success'
    },
    {
      value: 'FriendsOnly',
      label: '僅好友',
      description: '只有好友可以看到',
      icon: '👥',
      color: 'info'
    },
    {
      value: 'Private',
      label: '僅自己',
      description: '只有您可以看到',
      icon: '🔒',
      color: 'secondary'
    }
  ];

  constructor(private memberService: MemberService) {}

  ngOnInit(): void {
    this.loadPrivacySettings();
  }

  /**
   * 載入隱私設定
   */
  loadPrivacySettings(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.memberService.getPrivacySettings().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // 將資料轉換為 Map
          this.privacySettings.clear();
          this.originalSettings.clear();

          response.data.forEach((setting: PrivacySettingDto) => {
            this.privacySettings.set(setting.fieldName, setting.visibility);
            this.originalSettings.set(setting.fieldName, setting.visibility);
          });

          // 為沒有設定的欄位設定預設值
          this.privacyFields.forEach(field => {
            if (!this.privacySettings.has(field.fieldName)) {
              // 預設值：基本資訊為公開，私密資訊為僅自己
              const defaultValue = field.category === 'private' ? 'Private' : 'Public';
              this.privacySettings.set(field.fieldName, defaultValue);
              this.originalSettings.set(field.fieldName, defaultValue);
            }
          });
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('載入隱私設定失敗:', error);
        this.errorMessage = '載入隱私設定失敗，請稍後再試';
        this.isLoading = false;

        // 設定預設值
        this.setDefaultPrivacySettings();
      }
    });
  }

  /**
   * 設定預設隱私設定
   */
  private setDefaultPrivacySettings(): void {
    this.privacySettings.clear();
    this.originalSettings.clear();

    this.privacyFields.forEach(field => {
      const defaultValue = field.category === 'private' ? 'Private' : 'Public';
      this.privacySettings.set(field.fieldName, defaultValue);
      this.originalSettings.set(field.fieldName, defaultValue);
    });
  }

  /**
   * 取得欄位的可見性設定
   */
  getVisibility(fieldName: string): string {
    return this.privacySettings.get(fieldName) || 'Public';
  }

  /**
   * 變更欄位的可見性
   */
  changeVisibility(fieldName: string, visibility: string): void {
    this.privacySettings.set(fieldName, visibility);
    this.checkForChanges();
  }

  /**
   * 檢查是否有變更
   */
  private checkForChanges(): void {
    this.hasChanges = false;

    for (const [fieldName, visibility] of this.privacySettings.entries()) {
      const originalVisibility = this.originalSettings.get(fieldName);
      if (visibility !== originalVisibility) {
        this.hasChanges = true;
        break;
      }
    }
  }

  /**
   * 取得可見性選項的顯示資訊
   */
  getVisibilityOption(value: string): VisibilityOption | undefined {
    return this.visibilityOptions.find(option => option.value === value);
  }

  /**
   * 依分類篩選欄位
   */
  getFieldsByCategory(category: 'basic' | 'private' | 'contact'): PrivacyFieldConfig[] {
    return this.privacyFields.filter(field => field.category === category);
  }

  /**
   * 取得分類名稱
   */
  getCategoryName(category: string): string {
    const categoryNames: { [key: string]: string } = {
      'basic': '基本資訊',
      'private': '私密資訊',
      'contact': '聯絡資訊'
    };
    return categoryNames[category] || category;
  }

  /**
   * 取得分類圖示
   */
  getCategoryIcon(category: string): string {
    const categoryIcons: { [key: string]: string } = {
      'basic': '📋',
      'private': '🔐',
      'contact': '📞'
    };
    return categoryIcons[category] || '📁';
  }

  /**
   * 快速設定：全部公開
   */
  setAllPublic(): void {
    this.privacyFields.forEach(field => {
      this.privacySettings.set(field.fieldName, 'Public');
    });
    this.checkForChanges();
  }

  /**
   * 快速設定：全部僅好友
   */
  setAllFriendsOnly(): void {
    this.privacyFields.forEach(field => {
      this.privacySettings.set(field.fieldName, 'FriendsOnly');
    });
    this.checkForChanges();
  }

  /**
   * 快速設定：全部私密
   */
  setAllPrivate(): void {
    this.privacyFields.forEach(field => {
      this.privacySettings.set(field.fieldName, 'Private');
    });
    this.checkForChanges();
  }

  /**
   * 重設為預設值
   */
  resetToDefault(): void {
    this.privacySettings.clear();
    this.privacyFields.forEach(field => {
      const originalValue = this.originalSettings.get(field.fieldName);
      if (originalValue) {
        this.privacySettings.set(field.fieldName, originalValue);
      }
    });
    this.checkForChanges();
  }

  /**
   * 儲存隱私設定
   */
  savePrivacySettings(): void {
    if (!this.hasChanges) {
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    // 準備批次更新資料
    const settings: UpdatePrivacySettingDto[] = [];

    this.privacySettings.forEach((visibility, fieldName) => {
      settings.push({
        fieldName: fieldName,
        visibility: visibility
      });
    });

    const batchDto: BatchUpdatePrivacySettingsDto = {
      settings: settings
    };

    this.memberService.updatePrivacySettings(batchDto).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = '隱私設定已更新';

          // 更新原始設定
          this.originalSettings.clear();
          this.privacySettings.forEach((visibility, fieldName) => {
            this.originalSettings.set(fieldName, visibility);
          });

          this.hasChanges = false;

          // 3秒後自動隱藏成功訊息
          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        } else {
          this.errorMessage = response.message || '更新隱私設定失敗';
        }
        this.isSaving = false;
      },
      error: (error) => {
        console.error('更新隱私設定失敗:', error);
        this.errorMessage = error.error?.message || '更新隱私設定失敗，請稍後再試';
        this.isSaving = false;
      }
    });
  }

  /**
   * 關閉錯誤訊息
   */
  dismissError(): void {
    this.errorMessage = '';
  }

  /**
   * 關閉成功訊息
   */
  dismissSuccess(): void {
    this.successMessage = '';
  }
}
