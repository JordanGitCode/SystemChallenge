import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductsService } from '../services/product-service';
import { CurrentUser } from '../services/current-user';
import { ProductResponse } from '../models/product';

@Component({
  selector: 'app-products',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './products.html',
})
export class Products implements OnInit {
  private service = inject(ProductsService);
  protected readonly user = inject(CurrentUser);
  protected readonly products = signal<ProductResponse[]>([]);
  protected readonly error = signal<string | null>(null);

  ngOnInit() {
    this.load();
  }

  load() {
    this.service.getAll().subscribe({
      next: (p) => this.products.set(p),
      error: () => this.error.set('Failed to load products'),
    });
  }

  submit(p: ProductResponse) {
    this.service.submitForReview(p.id, p.currentVersionId).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.detail ?? 'Submit failed'),
    });
  }

  remove(p: ProductResponse) {
    if (!confirm(`Delete "${p.name}"?`)) return;
    this.service.remove(p.id).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.detail ?? 'Delete failed'),
    });
  }

  statusLabel(s: number): string {
    return ['Draft', 'Pending', 'Approved', 'Rejected'][s] ?? 'Unknown';
  }
}
