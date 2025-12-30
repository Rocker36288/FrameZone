import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SearchService {

  constructor() { }
  keyword = signal<string>('');

  setKeyword(value: string) {
    this.keyword.set(value.trim());
  }

  clear() {
    this.keyword.set('');
  }

}
