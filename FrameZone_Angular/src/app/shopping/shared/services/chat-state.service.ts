import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChatStateService {

  constructor() { }

  private _isOpen$ = new BehaviorSubject<boolean>(false);
  isOpen$ = this._isOpen$.asObservable();

  private _openedFromProduct$ = new BehaviorSubject<boolean>(false);
  openedFromProduct$ = this._openedFromProduct$.asObservable();

  private _product$ = new BehaviorSubject<any | null>(null);
  product$ = this._product$.asObservable();

  // 賣家頁相關的 Subject
  private _sellerName$ = new BehaviorSubject<string>('');
  sellerName$ = this._sellerName$.asObservable();

  private _sellerAvatar$ = new BehaviorSubject<string>('');
  sellerAvatar$ = this._sellerAvatar$.asObservable();

  openFromFloating() {
    this._openedFromProduct$.next(false);
    this._isOpen$.next(true);
  }

  /** 商品頁呼叫 */
  openFromProduct(product: any) {
    this._product$.next(product);
    this._openedFromProduct$.next(true);
    this._isOpen$.next(true);
  }

  /** 賣家頁呼叫 */
  openFromSeller(payload: { sellerName: string; sellerAvatar?: string }) {
    this._sellerName$.next(payload.sellerName);
    this._sellerAvatar$.next(payload.sellerAvatar || '');
    this._openedFromProduct$.next(false);
    this._isOpen$.next(true);
  }

  close() {
    this._isOpen$.next(false);
    this._openedFromProduct$.next(false);
    this._product$.next(null);
  }

}
