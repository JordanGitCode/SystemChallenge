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

  protected readonly items = signal<ProductReadModel[]>([]);
  protected readonly hasMore = signal(false);
  protected readonly error = signal<string | null>(null);
  private cursor = 0;

  ngOnInit() {
    this.load();
  }

  load() {
    this.service.getCatalog(this.cursor, 3).subscribe({
      next: (page) => {
        this.items.update((current) => [...current, ...page.items]);
        this.cursor = page.nextCursor;
        this.hasMore.set(page.hasMore);
      },
      error: () => this.error.set('Failed to load products'),
    });
  }
}
