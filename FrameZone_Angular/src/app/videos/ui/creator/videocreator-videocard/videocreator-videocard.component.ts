import { Component, Input } from '@angular/core';
import { VideoDurationPipe } from "../../../pipes/video-duration.pipe";
import { VideoTimeagoPipe } from "../../../pipes/video-timeago.pipe";
import { VideoDetailData } from '../../../models/videocreator-model';
import { PrivacyStatus, ProcessStatus } from '../../../models/video.enum';
import { NgClass } from '@angular/common';
@Component({
  selector: 'app-videocreator-videocard',
  imports: [VideoDurationPipe, VideoTimeagoPipe, NgClass],
  templateUrl: './videocreator-videocard.component.html',
  styleUrl: './videocreator-videocard.component.css'
})
export class VideocreatorVideocardComponent {
  onVideoClick() {
    throw new Error('Method not implemented.');
  }
  @Input() Video: VideoDetailData = {
    id: 0,
    title: '',
    description: '',
    thumbnail: '',
    duration: 0,
    uploadDate: new Date(),
    viewsCount: 0,
    likesCount: 0,
    commentCount: 0,
    videoUrl: '',
    processStatus: ProcessStatus.UPLOADING,
    privacyStatus: PrivacyStatus.PUBLIC
  }
}
