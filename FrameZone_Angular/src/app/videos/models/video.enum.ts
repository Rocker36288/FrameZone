// src/app/enums/video-status.enum.ts

// 流程狀態
export enum ProcessStatus {
  UPLOADING = 'UPLOADING',
  UPLOADED = 'UPLOADED',
  PRE_PROCESSING = 'PRE_PROCESSING',
  TRANSCODING = 'TRANSCODING',
  AI_AUDITING = 'AI_AUDITING',
  PUBLISHED = 'PUBLISHED',
  READY = 'READY',
  FAILED_TRANSCODE = 'FAILED_TRANSCODE',
  FAILED_AUDIT = 'FAILED_AUDIT'
}

// 隱私狀態
export enum PrivacyStatus {
  PUBLIC = 'PUBLIC',           // 公開
  UNLISTED = 'UNLISTED',       // 非公開 (僅網址)
  PRIVATE = 'PRIVATE',         // 私人 (僅自己)
  DRAFT = 'DRAFT'              // 草稿 (僅創作者工作頁)
}

export enum TargetTypeEnum {
  None = 0,
  Photo = 1,
  Social = 2,
  Shopping = 3,
  Studio = 4,
  Video = 5
}
