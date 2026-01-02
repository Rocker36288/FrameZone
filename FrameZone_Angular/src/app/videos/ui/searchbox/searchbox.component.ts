import { Component } from '@angular/core';
import { FormsModule } from "@angular/forms";
import { NgForOf, NgIf } from "@angular/common";
import { Router } from '@angular/router';

@Component({
  selector: 'app-searchbox',
  imports: [FormsModule, NgForOf, NgIf],
  templateUrl: './searchbox.component.html',
  styleUrl: './searchbox.component.css'
})
export class SearchboxComponent {
  keyword = '';
  sortBy: 'likes' | 'views' | 'date' = 'date';
  sortOrder: 'asc' | 'desc' = 'desc';

  constructor(private router: Router) { }

  search() {
    // 移除前後空白
    const trimmedKeyword = this.keyword.trim();

    this.router.navigate(['videos/search'], {
      queryParams: {
        keyword: trimmedKeyword || null,
        sortBy: this.sortBy,
        sortOrder: this.sortOrder
      }
    });
  }

  clearSearch() {
    this.keyword = '';
    // 選擇性：是否要在清除後立即搜尋
    // this.search();
  }

  sortLikes() {
    // 如果已經是按讚排序，則切換升降序
    if (this.sortBy === 'likes') {
      this.sortOrder = this.sortOrder === 'desc' ? 'asc' : 'desc';
    } else {
      this.sortBy = 'likes';
      this.sortOrder = 'desc';
    }
    this.search();
  }

  sortViews() {
    if (this.sortBy === 'views') {
      this.sortOrder = this.sortOrder === 'desc' ? 'asc' : 'desc';
    } else {
      this.sortBy = 'views';
      this.sortOrder = 'desc';
    }
    this.search();
  }

  sortDate(order: 'asc' | 'desc') {
    this.sortBy = 'date';
    this.sortOrder = order;
    this.search();
  }

  // 檢查當前是否為指定排序
  isActiveSort(sort: string): boolean {
    return this.sortBy === sort;
  }

  // 檢查當前排序方向
  isDescending(): boolean {
    return this.sortOrder === 'desc';
  }
}

