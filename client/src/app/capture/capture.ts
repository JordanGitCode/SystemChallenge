import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProductsService } from '../services/product-service';

@Component({
  selector: 'app-capture',
  imports: [ReactiveFormsModule],
  templateUrl: './capture.html',
})
export class Capture {
  private fb = inject(FormBuilder);
  private service = inject(ProductsService);
  private router = inject(Router);

  protected readonly error = signal<string | null>(null);
  protected readonly submitting = signal(false);

  protected form = this.fb.nonNullable.group({
    name:        ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required]],
    price:       [0,  [Validators.required, Validators.min(0.01)]],
    sku:         ['', [Validators.required, Validators.maxLength(64)]],
  });

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting.set(true);
    this.service.create(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/products']),
      error: () => { this.error.set('Failed to create product'); this.submitting.set(false); },
    });
  }
}