import { Component, inject, OnInit, signal } from '@angular/core';
import { ProductReadModel } from '../models/product-read';
import { ProductsService } from '../services/product-service';
import { CurrencyPipe, DatePipe } from '@angular/common';

@Component({
  selector: 'app-category-demo',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './category-demo.html',
  styleUrl: './category-demo.css',
})
export class CategoryDemo implements OnInit {
  private service = inject(ProductsService);
  protected readonly catalog = signal<ProductReadModel[]>([]);
  protected readonly error = signal<string | null>(null);

  ngOnInit() {
    this.load();
  }

  load() {
    this.service.getCatalog().subscribe({
      next: (p) => this.catalog.set(p),
      error: () => this.error.set('Failed to load products'),
    });
  }
}
