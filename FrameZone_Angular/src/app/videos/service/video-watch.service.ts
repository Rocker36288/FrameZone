import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { VideoCardData, VideoCommentModel } from '../models/video-model';

@Injectable({
  providedIn: 'root'
})
export class VideoWatchService {
  https = 'https://localhost:7213'
  constructor(private http: HttpClient) { }

  getVideo(guid: string) {
    return this.http.get<VideoCardData>(`${this.https}/api/videos/${guid}`);
  }

  getVideoComments(guid: string) {
    return this.http.get<VideoCommentModel>(`/api/videos/${guid}/comment`)
  }
}
