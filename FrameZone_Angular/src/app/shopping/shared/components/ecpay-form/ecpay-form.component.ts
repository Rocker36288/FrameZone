import { CommonModule, KeyValuePipe } from '@angular/common';
import { Component, ElementRef, Input, ViewChild } from '@angular/core';

@Component({
  selector: 'app-ecpay-form',
  standalone: true,
  imports: [CommonModule, KeyValuePipe],
  templateUrl: './ecpay-form.component.html',
  styleUrl: './ecpay-form.component.css'
})
export class EcpayFormComponent {
  // 綠界 API 地址（測試環境或正式環境）
  @Input() apiUrl: string = '';

  // 從後端 API 取得的所有參數（包含 CheckMacValue）
  @Input() postData: any = {};

  @ViewChild('paymentForm') paymentForm!: ElementRef<HTMLFormElement>;


  ngOnInit() {
    console.log('hello');

    // 當組件初始化且有資料時，自動送出
    if (this.postData && Object.keys(this.postData).length > 0) {
      setTimeout(() => this.submit(), 100);
    }
  }

  submit() {
    this.paymentForm.nativeElement.submit();
  }
}
