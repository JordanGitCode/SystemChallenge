import { Component, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CreateProductRequest } from '../models/create-product-request';

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule],
  templateUrl: './product-form.html',
})
export class ProductForm {
  private fb = inject(FormBuilder);

  readonly heading = input('');
  readonly note = input<string | null>(null);
  readonly submitLabel = input('Save');
  readonly submitting = input(false);
  readonly error = input<string | null>(null);
  readonly initialValue = input<CreateProductRequest | null>(null);

  readonly save = output<CreateProductRequest>();

  protected form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required]],
    price: [0, [Validators.required, Validators.min(0.01)]],
    sku: ['', [Validators.required, Validators.maxLength(64)]],
  });

  constructor() {
    // Pre-fill when a parent supplies a value (edit case).
    effect(() => {
      const v = this.initialValue();
      if (v) this.form.patchValue(v);
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.save.emit(this.form.getRawValue());
  }
}
