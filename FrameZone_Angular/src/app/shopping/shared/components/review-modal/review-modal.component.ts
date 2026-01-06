import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReviewService } from '../../services/review.service';
import { CreateReviewPayload } from '../../../interfaces/review';
import { ToastService } from '../../services/toast.service';

interface ReviewFormItem {
    orderId: number; // 雖然通常整單評價，但保留彈性
    productId: number;
    productName: string;
    productImage: string;
    spec: string; // 規格
    rating: number;
    content: string;
    images: File[];
    previewImages: string[];
}

@Component({
    selector: 'app-review-modal',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './review-modal.component.html',
    styleUrl: './review-modal.component.css'
})
export class ReviewModalComponent {
    @Input() visible: boolean = false;
    // 傳入訂單資料，我們會轉成 reviewItems
    @Input() set orderData(order: any) {
        if (order) {
            this.initForm(order);
        }
    }

    @Output() closeEvent = new EventEmitter<void>();
    @Output() submitSuccess = new EventEmitter<void>();

    reviewItems: ReviewFormItem[] = [];
    isSubmitting: boolean = false;

    constructor(
        private reviewService: ReviewService,
        private toastService: ToastService
    ) { }

    initForm(order: any) {
        this.reviewItems = order.products.map((p: any) => ({
            orderId: order.orderId || order.id, // 相容不同命名
            productId: p.productId || p.id,
            productName: p.name,
            productImage: p.imageUrl,
            spec: p.spec,
            rating: 5,
            content: '',
            images: [],
            previewImages: []
        }));
    }

    setRating(index: number, rating: number) {
        this.reviewItems[index].rating = rating;
    }

    triggerFileInput(index: number) {
        const fileInput = document.getElementById('file-input-' + index) as HTMLInputElement;
        if (fileInput) {
            fileInput.click();
        }
    }

    onFileSelected(event: any, index: number) {
        const files = event.target.files;
        if (files && files.length > 0) {
            const item = this.reviewItems[index];
            const remainingSlots = 3 - item.images.length;

            for (let i = 0; i < Math.min(files.length, remainingSlots); i++) {
                const file = files[i];

                // 檢查檔案大小 (限制 5MB)
                if (file.size > 5 * 1024 * 1024) {
                    this.toastService.show('圖片大小不能超過 5MB', 'top'); // 修正：ToastService 參數順序
                    continue;
                }

                item.images.push(file);

                const reader = new FileReader();
                reader.onload = (e: any) => {
                    item.previewImages.push(e.target.result);
                };
                reader.readAsDataURL(file);
            }
        }
        // 重置 input 以允許重複選取同檔案
        event.target.value = '';
    }

    removeImage(itemIndex: number, imgIndex: number) {
        this.reviewItems[itemIndex].images.splice(imgIndex, 1);
        this.reviewItems[itemIndex].previewImages.splice(imgIndex, 1);
    }

    isValid(): boolean {
        return this.reviewItems.every(item => item.rating > 0);
    }

    submit() {
        if (this.isSubmitting) return;

        this.isSubmitting = true;
        const payload: CreateReviewPayload[] = this.reviewItems.map(item => ({
            orderId: item.orderId,
            productId: item.productId,
            rating: item.rating,
            content: item.content,
            images: item.images
        }));

        this.reviewService.createReviews(payload).subscribe({
            next: () => {
                this.toastService.show('評價已送出！');
                this.submitSuccess.emit();
                this.close();
                this.isSubmitting = false;
            },
            error: (err) => {
                console.error('評價送出失敗', err);
                this.toastService.show('送出失敗，請稍後再試');
                this.isSubmitting = false;
            }
        });
    }

    close() {
        this.visible = false;
        this.closeEvent.emit();
    }
}
