// creator-analytics.component.ts
import { Component, OnInit } from '@angular/core';
import { NgForOf, NgIf } from "@angular/common";
import { CreatorAnalyticsDto } from '../../../models/videocreator-model';
import { VideoCreatorService } from '../../../service/video-creator.service';



@Component({
  selector: 'app-videocreator-analysis',
  imports: [NgForOf, NgIf],
  templateUrl: './videocreator-analysis.component.html',
  styleUrl: './videocreator-analysis.component.css'
})
export class VideocreatorAnalysisComponent implements OnInit {

  analytics!: CreatorAnalyticsDto;
  isLoading = true;
  selectedPeriod: '7days' | '30days' | '90days' = '7days';

  constructor(
    private videoCreatorService: VideoCreatorService
  ) { }

  ngOnInit(): void {
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    this.isLoading = true;

    this.videoCreatorService
      .getCreatorAnalytics(this.selectedPeriod)
      .subscribe({
        next: (data) => {
          this.analytics = data;
          this.isLoading = false;
          console.log(data)
        },
        error: (err) => {
          console.error('Analytics API error', err);
          this.isLoading = false;
        }
      });
  }

  changePeriod(period: '7days' | '30days' | '90days'): void {
    this.selectedPeriod = period;
    this.loadAnalytics();
  }

  formatNumber(num: number): string {
    if (num >= 1_000_000) return (num / 1_000_000).toFixed(1) + 'M';
    if (num >= 1_000) return (num / 1_000).toFixed(1) + 'K';
    return num.toString();
  }

  getEngagementRate(video: any): number {
    return ((video.likes + video.comments) / video.views * 100);
  }

  getMaxViews(): number {
    return this.analytics.performanceChart.length
      ? Math.max(...this.analytics.performanceChart.map(d => d.views))
      : 1; // 防止除以 0
  }
}
