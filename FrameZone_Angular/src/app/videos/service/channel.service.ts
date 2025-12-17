import { Injectable } from '@angular/core';
import { of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChannelService {

  private mockChannel = {
    Name: 'BBChannel',
    Avatar: 'https://placehold.co/160x160',
    Description: '5551',
    Follows: 111,
    Banner: '',
    CreatedAt: new Date(),
    LastUpdateAt: new Date(),
    VideosCount: 0
  };

  getChannel() {
    return of(this.mockChannel); // RxJS 假 API
  }
}
