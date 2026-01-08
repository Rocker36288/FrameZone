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
  private apiUrl = backendPublicUrl + '/api/order';

  //private apiUrl = 'https://localhost:7213/api/order';

  constructor() { }

  public createOrder(order: OrderDto): Observable<object> {
    console.log(order);
    return this.http.post<object>(this.apiUrl + '/create', order);
  }

  public getPickupStores(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl + '/pickup-stores');
  }

  public getMyOrders(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl + '/my-orders');
  }
}
