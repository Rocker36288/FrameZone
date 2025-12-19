import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class VideoUploadService {
  constructor(private http: HttpClient) { }

  getVideoStatus(videoGuid: string): Observable<any> {
    return this.http.get(`https://localhost:7213/api/videoupload/${videoGuid}/status`);
  }
}
