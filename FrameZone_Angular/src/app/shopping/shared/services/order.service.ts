import { CartItem } from './../../interfaces/cart';
import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { OrderDto } from '../../interfaces/order';
import { Observable } from 'rxjs';
import { frontendPublicUrl, backendPublicUrl } from '../configuration/url';


@Injectable({
  providedIn: 'root'
})
export class OrderService {

  private http = inject(HttpClient);
  private apiUrl = backendPublicUrl + '/api/order/create';

  constructor() { }

  public createOrder(order: OrderDto): Observable<object> {
    console.log(order);
    return this.http.post<object>(this.apiUrl, order);
  }
}
