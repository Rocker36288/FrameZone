import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ToastService {

  constructor() { }
  private toastSubject = new Subject<{ message: string, position: 'top' | 'bottom' }>();
  toastState$ = this.toastSubject.asObservable();

  show(message: string, position: 'top' | 'bottom' = 'bottom') {
    this.toastSubject.next({ message, position });
  }
}
