import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-report-button',
  imports: [],
  templateUrl: './report-button.component.html',
  styleUrl: './report-button.component.css'
})
export class ReportButtonComponent {
  @Input() videoid: number = 0
}
