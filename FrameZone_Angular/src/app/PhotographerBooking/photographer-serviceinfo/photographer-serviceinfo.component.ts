import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
interface Service {
  serviceId: number;
  serviceName: string;
  description: string;
  basePrice: number;
  duration: number;
  maxRevisions: number;
  deliveryDays: number;
  includedPhotos: number;
  isPopular?: boolean;
  originalPrice?: number;
}
interface FAQ {
  question: string;
  answer: string;
}
@Component({
  selector: 'app-photographer-serviceinfo',
  imports: [CommonModule],
  templateUrl: './photographer-serviceinfo.component.html',
  styleUrl: './photographer-serviceinfo.component.css',
})
export class PhotographerServiceinfoComponent {
  @Input() services: Service[] = [];
  @Input() faqs: FAQ[] = [];
  @Input() selectedService: Service | null = null;
  @Output() serviceSelected = new EventEmitter<Service>();

  expandedServiceId: number | null = null;

  onSelectService(service: Service): void {
    this.serviceSelected.emit(service);
  }

  toggleServiceDetails(serviceId: number): void {
    this.expandedServiceId =
      this.expandedServiceId === serviceId ? null : serviceId;
  }

  isServiceExpanded(serviceId: number): boolean {
    return this.expandedServiceId === serviceId;
  }
}
