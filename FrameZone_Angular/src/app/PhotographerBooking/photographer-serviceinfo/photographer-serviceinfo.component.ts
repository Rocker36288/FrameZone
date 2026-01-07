import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ServiceDto } from '../models/photographer-booking.models';

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
  @Input() services: ServiceDto[] = [];
  @Input() faqs: FAQ[] = [];
  @Input() selectedService: ServiceDto | null = null;
  @Output() serviceSelected = new EventEmitter<ServiceDto>();

  expandedServiceId: number | null = null;

  onSelectService(service: ServiceDto): void {
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
