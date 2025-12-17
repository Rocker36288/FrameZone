import { HttpClient, HttpEventType } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-videocreator-upload',
  imports: [],
  templateUrl: './videocreator-upload.component.html',
  styleUrl: './videocreator-upload.component.css'
})
export class VideocreatorUploadComponent {

  selectedFile: File | null = null;

  uploading = false;
  uploadProgress = 0;
  statusMessage = '上傳中...';

  constructor(private http: HttpClient) { }

  // ─────────────────────────────
  // 檔案選擇
  // ─────────────────────────────
  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.prepareUpload(file);
  }

  // ─────────────────────────────
  // 拖放處理
  // ─────────────────────────────
  onDragOver(event: DragEvent) {
    event.preventDefault();
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
  }

  onDrop(event: DragEvent) {
    event.preventDefault();

    const file = event.dataTransfer?.files?.[0];
    if (!file) return;

    this.prepareUpload(file);
  }

  // ─────────────────────────────
  // 上傳準備
  // ─────────────────────────────
  prepareUpload(file: File) {
    // 檢查格式
    if (!file.type.startsWith('video/')) {
      alert('請選擇影片檔案');
      return;
    }

    // 檢查大小（500MB）
    if (file.size > 500 * 1024 * 1024) {
      alert('檔案超過 500MB');
      return;
    }

    this.selectedFile = file;
    this.upload();
  }

  // ─────────────────────────────
  // 呼叫後端 API
  // ─────────────────────────────
  upload() {
    if (!this.selectedFile) return;

    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('title', this.selectedFile.name);

    this.uploading = true;
    this.uploadProgress = 0;
    this.statusMessage = '上傳中...';

    this.http.post('/api/videos/upload', formData, {
      observe: 'events',
      reportProgress: true
    }).subscribe({
      next: event => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.uploadProgress = Math.round(
            (event.loaded / event.total) * 100
          );
        }

        if (event.type === HttpEventType.Response) {
          this.statusMessage = '上傳完成，影片處理中...';
        }
      },
      error: () => {
        this.uploading = false;
        this.statusMessage = '上傳失敗';
      },
      complete: () => {
        // 後續可以導向影片管理頁
      }
    });
  }
}
