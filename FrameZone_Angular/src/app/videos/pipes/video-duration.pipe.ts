import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'videoDuration', // 使用方式 {{ durationInSeconds | videoDuration }}
  standalone: true,
})
export class VideoDurationPipe implements PipeTransform {

  transform(value: number): string {
    if (value == null || isNaN(value)) return '00:00';

    const hours = Math.floor(value / 3600);
    const minutes = Math.floor((value % 3600) / 60);
    const seconds = Math.floor(value % 60);

    const pad = (n: number) => n.toString().padStart(2, '0');

    // 若有小時才顯示 HH
    if (hours > 0) {
      return `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
    }

    return `${pad(minutes)}:${pad(seconds)}`;
  }
}
