import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-photographer-servicetype',
  imports: [CommonModule],
  templateUrl: './photographer-servicetype.component.html',
  styleUrl: './photographer-servicetype.component.css',
})
export class PhotographerServicetypeComponent {
  categories = [
    {
      name: '婚禮攝影',
      img: 'images/photo-1494526585095-c41746248156.avif',
    },
    {
      name: '人像寫真',
      img: 'images/photo-1502672260266-1c1ef2d93688.avif',
    },
    {
      name: '商業攝影',
      img: 'images/photo-1512917774080-9991f1c4c750.avif',
    },
    {
      name: '寵物攝影',
      img: 'images/photographercard001.png',
    },
  ];
}
