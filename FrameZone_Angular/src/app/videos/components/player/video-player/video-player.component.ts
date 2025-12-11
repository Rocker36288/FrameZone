import { Component, ElementRef, Input, ViewChild, OnDestroy, AfterViewInit } from '@angular/core';
import Hls from 'hls.js';
import { NgIf, NgFor, NgClass } from '@angular/common';

interface QualityLevel {
  index: number;
  height: number;
  bitrate: number;
  name: string;
}

@Component({
  selector: 'app-video-player',
  imports: [NgIf, NgFor, NgClass],
  templateUrl: './video-player.component.html',
  styleUrl: './video-player.component.css'
})
export class VideoPlayerComponent implements AfterViewInit, OnDestroy {
  @ViewChild('videoRef') videoRef!: ElementRef<HTMLVideoElement>;
  @ViewChild('videoContainer') videoContainer!: ElementRef<HTMLDivElement>;
  @Input() src!: string;

  private hls?: Hls;
  private hideControlsTimeout?: any;


  qualityLevels: QualityLevel[] = [];
  currentQuality: number = -1;
  showQualityMenu: boolean = false;
  showControls: boolean = false;
  hideCursor: boolean = false; // 新增

  ngAfterViewInit(): void {
    const video = this.videoRef.nativeElement;

    if (Hls.isSupported()) {
      this.hls = new Hls({
        enableWorker: true,
        lowLatencyMode: true,
      });

      this.hls.loadSource(this.src);
      this.hls.attachMedia(video);

      this.hls.on(Hls.Events.MANIFEST_PARSED, async () => {
        this.loadQualityLevels();
        this.initCustomControls();

        try {
          await video.play();
          console.log('有聲音自動播放成功');
        } catch (err) {
          console.log('有聲音播放失敗，改為靜音:', err);
          video.muted = true;
          await video.play();
        }
      });

      this.hls.on(Hls.Events.LEVEL_SWITCHED, (event, data) => {
        console.log('已切換到畫質:', data.level);
        this.currentQuality = this.hls!.currentLevel;
      });
    } else if (video.canPlayType('application/vnd.apple.mpegurl')) {
      video.src = this.src;
      video.addEventListener('loadedmetadata', () => {
        this.initCustomControls();
      });
    }

    // 監聽影片播放/暫停來控制控制列顯示
    video.addEventListener('play', () => this.hideControlsAfterDelay());
    video.addEventListener('pause', () => this.showControls = true);
  }

  private initCustomControls(): void {
    const video = this.videoRef.nativeElement;

    // 隱藏原生控制列
    video.removeAttribute('controls');

    // 點擊影片播放/暫停
    video.addEventListener('click', () => {
      if (video.paused) {
        video.play();
      } else {
        video.pause();
      }
    });
  }

  private hideControlsAfterDelay(): void {
    if (this.hideControlsTimeout) {
      clearTimeout(this.hideControlsTimeout);
    }
    this.hideControlsTimeout = setTimeout(() => {
      if (!this.videoRef.nativeElement.paused && !this.showQualityMenu) {
        this.showControls = false;
        this.hideCursor = true;
      }
    }, 3000);
  }

  private loadQualityLevels(): void {
    if (!this.hls) return;

    this.qualityLevels = this.hls.levels.map((level, index) => ({
      index: index,
      height: level.height,
      bitrate: level.bitrate,
      name: this.getQualityName(level.height)
    }));

    this.qualityLevels.sort((a, b) => b.height - a.height);
  }

  private getQualityName(height: number): string {
    if (height >= 2160) return '4K';
    if (height >= 1440) return '2K';
    if (height >= 1080) return '1080p';
    if (height >= 720) return '720p';
    if (height >= 480) return '480p';
    if (height >= 360) return '360p';
    return `${height}p`;
  }

  changeQuality(levelIndex: number): void {
    if (!this.hls) return;

    if (levelIndex === -1) {
      this.hls.currentLevel = -1;
      console.log('已切換為自動畫質');
    } else {
      this.hls.currentLevel = levelIndex;
      console.log('已切換為:', this.qualityLevels.find(q => q.index === levelIndex)?.name);
    }

    this.currentQuality = levelIndex;
    this.showQualityMenu = false;
  }

  toggleQualityMenu(): void {
    this.showQualityMenu = !this.showQualityMenu;
  }

  getCurrentQualityText(): string {
    if (this.currentQuality === -1) {
      return '自動';
    }
    const quality = this.qualityLevels.find(q => q.index === this.currentQuality);
    return quality ? quality.name : '自動';
  }

  onMouseEnter(): void {
    this.showControls = true;
  }

  onMouseMove(): void {
    this.showControls = true;
    this.hideCursor = false; // 鼠標移動時顯示游標
    if (!this.videoRef.nativeElement.paused && !this.showQualityMenu) {
      this.hideControlsAfterDelay();
    }
  }

  onMouseLeave(): void {
    if (!this.showQualityMenu) {
      this.showControls = false;
    }
  }

  // 播放/暫停
  togglePlay(): void {
    const video = this.videoRef.nativeElement;
    if (video.paused) {
      video.play();
    } else {
      video.pause();
    }
  }

  // 全螢幕
  toggleFullscreen(): void {
    const container = this.videoContainer.nativeElement;

    if (!document.fullscreenElement) {
      container.requestFullscreen();
    } else {
      document.exitFullscreen();
    }
  }

  // 音量控制
  toggleMute(): void {
    const video = this.videoRef.nativeElement;
    video.muted = !video.muted;
  }

  // 格式化時間
  formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  // 獲取當前時間
  getCurrentTime(): string {
    return this.formatTime(this.videoRef?.nativeElement?.currentTime || 0);
  }

  // 獲取總時長
  getDuration(): string {
    return this.formatTime(this.videoRef?.nativeElement?.duration || 0);
  }

  // 進度條變更
  onProgressChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const video = this.videoRef.nativeElement;
    video.currentTime = (parseFloat(input.value) / 100) * video.duration;
  }

  // 獲取當前進度百分比
  getProgress(): number {
    const video = this.videoRef?.nativeElement;
    if (!video || !video.duration) return 0;
    return (video.currentTime / video.duration) * 100;
  }

  // 音量變更
  onVolumeChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const video = this.videoRef.nativeElement;
    video.volume = parseFloat(input.value) / 100;
    video.muted = video.volume === 0;
  }

  // 獲取音量
  getVolume(): number {
    return (this.videoRef?.nativeElement?.volume || 1) * 100;
  }

  // 是否靜音
  isMuted(): boolean {
    return this.videoRef?.nativeElement?.muted || false;
  }

  // 是否播放中
  isPlaying(): boolean {
    return !this.videoRef?.nativeElement?.paused;
  }

  ngOnDestroy(): void {
    if (this.hls) {
      this.hls.destroy();
    }
  }
}
