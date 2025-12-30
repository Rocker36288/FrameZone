import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { PhotoService } from '../../../core/services/photo.service';
import { UploadFileItem, PhotoMetadata } from '../../../core/models/photo.models';
import { firstValueFrom } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { PhotoConstants } from '../../../shared/constants/photo.constants';

@Component({
  selector: 'app-photo-classify',
  imports: [CommonModule],
  templateUrl: './photo-classify.component.html',
  styleUrl: './photo-classify.component.css'
})
export class PhotoClassifyComponent {
  uploadFiles = signal<UploadFileItem[]>([]);
  isDragging = signal(false);
  isUploading = signal(false);
  uploadProgress = signal(0);

  totalFiles = signal(0);
  successCount = signal(0);
  failedCount = signal(0);

  // 篩選狀態
  filterStatus = signal<'all' | 'pending' | 'success' | 'error'>('all');

  pendingFilesCount = computed(() =>
    this.uploadFiles().filter(f => f.status === 'pending').length
  );

  // 根據篩選狀態過濾檔案
  filteredFiles = computed(() => {
    const status = this.filterStatus();
    if (status === 'all') {
      return this.uploadFiles();
    }
    return this.uploadFiles().filter(f => f.status === status);
  });

  // 建議留 buffer：後端 batch-upload 上限 200MB，你用 180MB 比較安全
  private readonly MAX_BATCH_BYTES = PhotoConstants.MAX_BATCH_TOTAL_SIZE_BYTES * 0.9;

  private buildBatches(items: UploadFileItem[]): UploadFileItem[][] {
    const batches: UploadFileItem[][] = [];
    let current: UploadFileItem[] = [];
    let currentBytes = 0;

    for (const item of items) {
      const size = item.file.size;

      // 防呆：若單檔超過上限（你前端已限制 50MB，通常不會發生）
      if (size > this.MAX_BATCH_BYTES) {
        batches.push([item]);
        continue;
      }

      if (current.length > 0 && currentBytes + size > this.MAX_BATCH_BYTES) {
        batches.push(current);
        current = [];
        currentBytes = 0;
      }

      current.push(item);
      currentBytes += size;
    }

    if (current.length > 0) batches.push(current);
    return batches;
  }

  // 取得上傳限制資訊（用於 HTML 顯示）
  get uploadLimits() {
    return {
      maxFileSize: PhotoConstants.MAX_FILE_SIZE_MB,
      maxBatchCount: PhotoConstants.MAX_BATCH_UPLOAD_COUNT,
      allowedExtensions: PhotoConstants.ALLOWED_IMAGE_EXTENSIONS.join(', ')
    };
  }


  constructor(
    private photoService: PhotoService,
    private toastr: ToastrService
  ) {
    console.log('📦 ToastrService 注入狀態:', this.toastr ? '成功' : '失敗');
  }

  /**
   * 檔案拖移
   */
  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  /**
   * 檔案拖移離開
   */
  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  /**
   * 檔案拖放
   */
  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);

    const files = event.dataTransfer?.files;
    if (files) {
      this.handleFiles(Array.from(files));
    }
  }

  /**
   * 檔案選擇 (透過 input)
   */
  onFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(Array.from(input.files));
    }
  }

  /**
   * 處理選擇的檔案
   */
  async handleFiles(files: File[]) {
    console.log('📄 開始處理檔案，數量:', files.length);

    if (!PhotoConstants.isBatchCountValid(files.length)) {
      this.toastr.error(
        PhotoConstants.getBatchCountExceededMessage(),
        '批次上傳限制'
      );
      return;
    }

    const currentFiles = this.uploadFiles();

    let addedCount = 0;
    let duplicateInListCount = 0;
    let invalidCount = 0;

    for (const file of files) {
      console.log(`\n📄 處理檔案: ${file.name}`);

      // 驗證檔案格式
      const validation = this.photoService.validateFile(file);

      if (!validation.valid) {
        console.log(`❌ 檔案驗證失敗: ${validation.error}`);
        invalidCount++;
        currentFiles.push({
          file,
          fileName: file.name,
          fileSize: file.size,
          status: 'error',
          progress: 0,
          error: validation.error
        });
        continue;
      }

      // 計算 Hash
      let fileHash: string;
      try {
        console.log('🔐 開始計算 Hash...');
        fileHash = await this.photoService.calculateFileHash(file);
        console.log('✅ Hash 計算完成:', fileHash.substring(0, 16) + '...');
      } catch (error) {
        console.error('❌ 計算 Hash 失敗:', error);
        invalidCount++;
        currentFiles.push({
          file,
          fileName: file.name,
          fileSize: file.size,
          status: 'error',
          progress: 0,
          error: '無法計算檔案指紋'
        });
        continue;
      }

      // ⚠️ 只檢查當前上傳清單中的重複（不查資料庫）
      const duplicateInList = currentFiles.find(f => f.hash === fileHash);
      if (duplicateInList) {
        console.log('⚠️ 在上傳清單中發現重複');
        duplicateInListCount++;
        currentFiles.push({
          file,
          fileName: file.name,
          fileSize: file.size,
          status: 'error',
          progress: 0,
          error: '此照片已在上傳清單中',
          hash: fileHash
        });
        continue;
      }

      // ✅ 建立上傳項目（不檢查資料庫，交給後端處理）
      const uploadItem: UploadFileItem = {
        file,
        fileName: file.name,
        fileSize: file.size,
        status: 'pending',
        progress: 0,
        hash: fileHash
      };

      // 產生預覽圖
      try {
        uploadItem.preview = await this.photoService.generatePreview(file);
      } catch (error) {
        console.error('產生預覽圖失敗:', error);
      }

      addedCount++;
      currentFiles.push(uploadItem);
    }

    this.uploadFiles.set([...currentFiles]);

    console.log('\n📊 檔案處理統計:');
    console.log('  總檔案數:', files.length);
    console.log('  成功加入:', addedCount);
    console.log('  清單重複:', duplicateInListCount);
    console.log('  格式無效:', invalidCount);

    // 顯示處理結果
    if (addedCount > 0) {
      this.toastr.success(`已加入 ${addedCount} 張照片到上傳清單`, '✔ 成功');
    }

    if (duplicateInListCount > 0) {
      this.toastr.warning(
        `${duplicateInListCount} 張照片在清單中重複`,
        '⚠ 清單重複'
      );
    }

    if (invalidCount > 0) {
      this.toastr.error(
        `${invalidCount} 張照片格式不符或無效`,
        '✗ 無效檔案'
      );
    }
  }

  /**
   * 從成功清單中移除
   */
  removeSuccessFiles() {
    const files = this.uploadFiles();
    const remainingFiles = files.filter(f => f.status !== 'success');
    this.uploadFiles.set([...remainingFiles]);
  }

  /**
   * 移除檔案
   */
  removeFile(index: number) {
    const files = this.uploadFiles();
    files.splice(index, 1);
    this.uploadFiles.set([...files]);
  }

  /**
   * 清空所有檔案
   */
  clearAll() {
    this.uploadFiles.set([]);
    this.resetStats();

    const fileInputs = document.querySelectorAll('input[type="file"]');
    fileInputs.forEach((input: any) => {
      input.value = '';
    })
  }

  /**
   * 開始批次上傳
   */
  async startBatchUpload() {
    if (this.isUploading()) {
      console.log('⚠️ 已經在上傳中，忽略重複呼叫');
      return;
    }

    const files = this.uploadFiles();
    const validFiles = files.filter(f => f.status === 'pending');

    console.log('🚀 開始批次上傳');
    console.log('   總檔案數:', files.length);
    console.log('   待上傳檔案數:', validFiles.length);

    if (validFiles.length === 0) {
      this.toastr.warning('沒有可上傳的檔案', '提示');
      return;
    }

    this.isUploading.set(true);
    this.resetStats();
    this.totalFiles.set(validFiles.length);

    try {
      // 逐張上傳，即時顯示進度
      for (const fileItem of validFiles) {
        console.log(`\n📤 開始上傳: ${fileItem.fileName}`);

        // 標記為上傳中
        fileItem.status = 'uploading';
        fileItem.progress = 0;
        this.uploadFiles.set([...files]);

        try {
          // 單張上傳
          const response = await firstValueFrom(
            this.photoService.batchUpload([fileItem.file])
          );

          console.log(`📊 上傳回應:`, response);

          // 處理單張結果
          if (response && response.results && response.results.length > 0) {
            const result = response.results[0];

            if (result.success) {
              fileItem.status = 'success';
              fileItem.photoId = result.photoId;
              fileItem.progress = 100;
              this.successCount.set(this.successCount() + 1);
              console.log(`✅ ${fileItem.fileName} 上傳成功`);
            } else {
              fileItem.status = 'error';

              // 判斷是否為重複錯誤
              const errorMsg = result.error || '上傳失敗';
              const isDuplicate =
                errorMsg.includes('已經上傳過') ||
                errorMsg.includes('重複') ||
                errorMsg.includes('duplicate') ||
                errorMsg.includes('already uploaded');

              if (isDuplicate) {
                fileItem.error = '此照片已存在於相簿中';
                console.log(`⚠️ ${fileItem.fileName} 重複上傳`);
              } else {
                fileItem.error = errorMsg;
                console.log(`❌ ${fileItem.fileName} 上傳失敗: ${errorMsg}`);
              }

              this.failedCount.set(this.failedCount() + 1);
            }
          } else {
            // 無有效回應
            fileItem.status = 'error';
            fileItem.error = '伺服器無回應';
            this.failedCount.set(this.failedCount() + 1);
          }

        } catch (error: unknown) {
          console.error(`❌ ${fileItem.fileName} 上傳錯誤:`, error);
          fileItem.status = 'error';

          const errorMessage = error instanceof Error
            ? error.message
            : (error as any)?.error?.message || '網路錯誤';

          fileItem.error = errorMessage;
          this.failedCount.set(this.failedCount() + 1);
        }

        // 即時更新 UI
        this.uploadFiles.set([...files]);

        // 短暫延遲，避免請求過快
        await new Promise(resolve => setTimeout(resolve, 100));
      }

      // 顯示總結
      const successCount = this.successCount();
      const failedCount = this.failedCount();
      const duplicateCount = validFiles.filter(
        f => f.error?.includes('已存在於相簿中')
      ).length;
      const otherErrorCount = failedCount - duplicateCount;

      console.log('\n📊 上傳結果統計:');
      console.log('  成功:', successCount);
      console.log('  重複:', duplicateCount);
      console.log('  其他錯誤:', otherErrorCount);

      if (successCount > 0) {
        this.toastr.success(
          `成功上傳 ${successCount} 張照片`,
          '✔ 上傳完成'
        );
      }

      if (duplicateCount > 0) {
        this.toastr.warning(
          `${duplicateCount} 張照片已存在相簿中，已自動略過`,
          '⚠ 重複照片'
        );
      }

      if (otherErrorCount > 0) {
        this.toastr.error(
          `${otherErrorCount} 張照片上傳失敗`,
          '✗ 上傳錯誤'
        );
      }

    } catch (error: unknown) {
      console.error('❌ 批次上傳錯誤:', error);
      const errorMessage = error instanceof Error
        ? error.message
        : (error as any)?.error?.message || '未知錯誤';
      this.toastr.error(`上傳失敗: ${errorMessage}`, '錯誤');
    } finally {
      this.isUploading.set(false);

      // 清除檔案選擇器
      const fileInputs = document.querySelectorAll('input[type="file"]');
      fileInputs.forEach((input: any) => {
        input.value = '';
      });
    }
  }

  /**
   * 處理批次上傳回應
   */
  private handleBatchUploadResponse(response: any) {
    console.log('📊 處理批次上傳回應:', response);

    const files = this.uploadFiles();

    this.successCount.set(this.successCount() + (response.successCount || 0));
    this.failedCount.set(this.failedCount() + (response.failedCount || 0));

    let duplicateCount = 0;
    let otherErrorCount = 0;

    // 更新每個檔案的狀態
    response.results?.forEach((result: any) => {
      const fileItem = files.find(f => f.fileName === result.fileName);
      if (fileItem) {
        if (result.success) {
          fileItem.status = 'success';
          fileItem.photoId = result.photoId;
          fileItem.progress = 100;
          console.log(`✅ ${result.fileName} 上傳成功`);
        } else {
          fileItem.status = 'error';

          // 判斷是否為重複錯誤
          const errorMsg = result.error || '上傳失敗';
          const isDuplicate =
            errorMsg.includes('已經上傳過') ||
            errorMsg.includes('重複') ||
            errorMsg.includes('duplicate') ||
            errorMsg.includes('already uploaded');

          if (isDuplicate) {
            duplicateCount++;
            fileItem.error = '此照片已存在於相簿中';
            console.log(`⚠️ ${result.fileName} 重複上傳`);
          } else {
            otherErrorCount++;
            fileItem.error = errorMsg;
            console.log(`❌ ${result.fileName} 上傳失敗: ${errorMsg}`);
          }
        }
      }
    });

    this.uploadFiles.set([...files]);

    console.log('\n📊 上傳結果統計:');
    console.log('  成功:', response.successCount);
    console.log('  重複:', duplicateCount);
    console.log('  其他錯誤:', otherErrorCount);

    // 顯示結果通知
    if (response.successCount > 0) {
      this.toastr.success(
        `成功上傳 ${response.successCount} 張照片`,
        '✔ 上傳完成'
      );
    }

    if (duplicateCount > 0) {
      this.toastr.warning(
        `${duplicateCount} 張照片已存在相簿中，已自動略過`,
        '⚠ 重複照片'
      );
    }

    if (otherErrorCount > 0) {
      this.toastr.error(
        `${otherErrorCount} 張照片上傳失敗`,
        '✗ 上傳錯誤'
      );
    }

    const fileInputs = document.querySelectorAll('input[type="file"]');
    fileInputs.forEach((input: any) => {
      input.value = '';
    });
  }

  /**
   * 重設統計資訊
   */
  private resetStats() {
    this.totalFiles.set(0);
    this.successCount.set(0);
    this.failedCount.set(0);
    this.uploadProgress.set(0);
  }

  /**
   * 格式化檔案大小
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  /**
   * 取得狀態圖示 class
   */
  getStatusIconClass(status: string): string {
    switch (status) {
      case 'pending': return 'ti ti-clock text-secondary';
      case 'uploading': return 'ti ti-loader-2 text-primary';
      case 'success': return 'ti ti-check text-success';
      case 'error': return 'ti ti-x text-danger';
      default: return 'ti ti-question-mark';
    }
  }

  /**
   * 取得狀態文字
   */
  getStatusText(status: string): string {
    switch (status) {
      case 'pending': return '等待上傳';
      case 'uploading': return '上傳中...';
      case 'success': return '上傳成功';
      case 'error': return '上傳失敗';
      default: return '未知狀態';
    }
  }

  /**
   * 取得狀態文字顏色 class
   */
  getStatusTextClass(status: string): string {
    switch (status) {
      case 'pending': return 'status-pending';
      case 'uploading': return 'status-uploading';
      case 'success': return 'status-success';
      case 'error': return 'status-error';
      default: return '';
    }
  }

  /**
   * 設定篩選狀態
   */
  setFilterStatus(status: 'all' | 'pending' | 'success' | 'error') {
    this.filterStatus.set(status);
  }

  /**
   * 重試失敗的檔案
   */
  async retryFailedFiles() {
    const failedFiles = this.uploadFiles().filter(f => f.status === 'error');

    if (failedFiles.length === 0) {
      this.toastr.info('沒有需要重試的檔案', '提示');
      return;
    }

    // 重置失敗檔案的狀態
    this.uploadFiles().forEach(f => {
      if (f.status === 'error') {
        f.status = 'pending';
        f.error = undefined;
      }
    });
    this.uploadFiles.set([...this.uploadFiles()]);

    this.toastr.info(`已重置 ${failedFiles.length} 個失敗項目，請點擊「開始批次上傳」`, '重試準備');
  }

  /**
   * 取得檔案類型圖示
   */
  getFileTypeIcon(fileName: string): string {
    const ext = fileName.split('.').pop()?.toLowerCase() || '';

    switch (ext) {
      case 'jpg':
      case 'jpeg':
        return 'ti ti-file-type-jpg';
      case 'png':
        return 'ti ti-file-type-png';
      case 'heic':
      case 'heif':
        return 'ti ti-device-mobile';
      case 'gif':
        return 'ti ti-gif';
      case 'bmp':
        return 'ti ti-photo';
      case 'webp':
        return 'ti ti-brand-chrome';
      default:
        return 'ti ti-photo';
    }
  }

  /**
   * 取得檔案類型標籤 class
   */
  getFileTypeBadgeClass(fileName: string): string {
    const ext = fileName.split('.').pop()?.toLowerCase() || '';

    switch (ext) {
      case 'jpg':
      case 'jpeg':
        return 'badge-jpg';
      case 'png':
        return 'badge-png';
      case 'heic':
      case 'heif':
        return 'badge-heic';
      case 'gif':
        return 'badge-gif';
      default:
        return '';
    }
  }
}
