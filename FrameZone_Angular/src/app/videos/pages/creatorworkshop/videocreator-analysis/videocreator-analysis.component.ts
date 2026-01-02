// creator-analytics.component.ts
import { Component, OnInit } from '@angular/core';
import { NgForOf, NgIf } from "@angular/common";

interface AnalyticsData {
  totalViews: number;
  totalSubscribers: number;
  totalVideos: number;
  avgEngagement: number;
  viewsGrowth: number;
  subscribersGrowth: number;
  recentVideos: VideoData[];
  performanceChart: ChartData[];
}

interface VideoData {
  id: number;
  title: string;
  views: number;
  likes: number;
  comments: number;
  publishDate: string;
  thumbnail: string;
}

interface ChartData {
  date: string;
  views: number;
  engagement: number;
}

@Component({
  selector: 'app-videocreator-analysis',
  imports: [NgForOf, NgIf],
  templateUrl: './videocreator-analysis.component.html',
  styleUrl: './videocreator-analysis.component.css'
})
export class VideocreatorAnalysisComponent implements OnInit {
  analytics: AnalyticsData = {
    totalViews: 0,
    totalSubscribers: 0,
    totalVideos: 0,
    avgEngagement: 0,
    viewsGrowth: 0,
    subscribersGrowth: 0,
    recentVideos: [],
    performanceChart: []
  };

  isLoading = true;
  selectedPeriod = '7days';

  ngOnInit(): void {
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    this.isLoading = true;

    // 模擬 API 調用
    setTimeout(() => {
      this.analytics = {
        totalViews: 1284560,
        totalSubscribers: 45320,
        totalVideos: 128,
        avgEngagement: 8.5,
        viewsGrowth: 12.5,
        subscribersGrowth: 8.3,
        recentVideos: [
          {
            id: 1,
            title: '如何提升影片點閱率：5個實用技巧',
            views: 45200,
            likes: 3840,
            comments: 521,
            publishDate: '2025-12-28',
            thumbnail: 'https://via.placeholder.com/320x180/667eea/ffffff?text=Video+1'
          },
          {
            id: 2,
            title: '年度創作回顧與2026規劃',
            views: 38900,
            likes: 3200,
            comments: 445,
            publishDate: '2025-12-25',
            thumbnail: 'https://via.placeholder.com/320x180/764ba2/ffffff?text=Video+2'
          },
          {
            id: 3,
            title: '最新剪輯軟體教學：進階特效篇',
            views: 52100,
            likes: 4350,
            comments: 678,
            publishDate: '2025-12-22',
            thumbnail: 'https://via.placeholder.com/320x180/667eea/ffffff?text=Video+3'
          }
        ],
        performanceChart: [
          { date: '12/25', views: 12500, engagement: 7.8 },
          { date: '12/26', views: 15200, engagement: 8.2 },
          { date: '12/27', views: 18900, engagement: 8.9 },
          { date: '12/28', views: 21300, engagement: 9.1 },
          { date: '12/29', views: 19800, engagement: 8.7 },
          { date: '12/30', views: 23400, engagement: 9.3 },
          { date: '12/31', views: 25600, engagement: 9.5 }
        ]
      };
      this.isLoading = false;
    }, 1000);
  }

  changePeriod(period: string): void {
    this.selectedPeriod = period;
    this.loadAnalytics();
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  getEngagementRate(video: VideoData): number {
    return ((video.likes + video.comments) / video.views * 100);
  }
}
