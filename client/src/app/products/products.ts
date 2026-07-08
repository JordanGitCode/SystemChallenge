import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { ProductsService } from '../services/product-service';
import { ProductResponse } from '../models/product';

@Component({
  selector: 'app-products',
  imports: [CurrencyPipe],
  templateUrl: './products.html',
})
export class Products implements OnInit {

  private service = inject(ProductsService);
  protected readonly products = signal<ProductResponse[]>([]);
  protected readonly error = signal<string | null>(null);

  ngOnInit() {
    this.service.getAll().subscribe({
      next: p => this.products.set(p),
      error: () => this.error.set('Failed to load products'),
    });
  }

  statusLabel(s: number): string {
    return ['Draft', 'Pending', 'Approved', 'Rejected'][s] ?? 'Unknown';
  }
}