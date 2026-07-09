import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductsService } from '../services/product-service';
import { ProductForm } from '../shared/product-form';
import { CreateProductRequest } from '../models/create-product-request';

@Component({
  selector: 'app-edit',
  imports: [ProductForm],
  templateUrl: './edit.html',
})
export class Edit implements OnInit {
  private service = inject(ProductsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  private id = '';
  protected readonly initial = signal<CreateProductRequest | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly submitting = signal(false);

  ngOnInit() {
    this.id = this.route.snapshot.paramMap.get('id') ?? '';
    this.service.getById(this.id).subscribe({
      next: (p) =>
        this.initial.set({
          name: p.name,
          description: p.description,
          price: p.price,
          sku: p.sku,
        }),
      error: (err) => this.error.set(err.error?.detail ?? 'Failed to load product'),
    });
  }

  save(req: CreateProductRequest) {
    this.submitting.set(true);
    this.service.update(this.id, req).subscribe({
      next: () => this.router.navigate(['/products']),
      error: (err) => {
        this.error.set(err.error?.detail ?? 'Update failed');
        this.submitting.set(false);
      },
    });
  }
}
