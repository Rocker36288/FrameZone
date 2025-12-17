import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'videoTimeago',
  standalone: true
})
export class VideoTimeagoPipe implements PipeTransform {
  transform(value: Date | string | number): string {
    if (!value) return '未知時間';

    const date = new Date(value);
    const now = new Date();
    const diff = (now.getTime() - date.getTime()) / 1000; // 秒

    if (isNaN(diff) || diff < 0) return '未知時間';

    if (diff < 60) return '剛剛';                        // < 1 分鐘
    if (diff < 3600) return `${Math.floor(diff / 60)} 分鐘前`; // < 1 小時
    if (diff < 86400) return `${Math.floor(diff / 3600)} 小時前`; // < 1 天
    if (diff < 2592000) return `${Math.floor(diff / 86400)} 天前`; // < 30 天
    if (diff < 31536000) return `${Math.floor(diff / 2592000)} 個月前`; // < 1 年

    return `${date.getFullYear()}年${date.getUTCMonth() + 1}月${date.getUTCDate()}日`;
  }

}
