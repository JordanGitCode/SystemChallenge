import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ProductsService } from '../services/product-service';
import { ProductForm } from '../shared/product-form';
import { CreateProductRequest } from '../models/create-product-request';

@Component({
  selector: 'app-capture',
  imports: [ProductForm],
  templateUrl: './capture.html',
})
export class Capture {
  private service = inject(ProductsService);
  private router = inject(Router);

  protected readonly error = signal<string | null>(null);
  protected readonly submitting = signal(false);

  create(req: CreateProductRequest) {
    this.submitting.set(true);
    this.service.create(req).subscribe({
      next: () => this.router.navigate(['/products']),
      error: (err) => {
        this.error.set(err.error?.detail ?? 'Failed to create product');
        this.submitting.set(false);
      },
    });
  }
}
