import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ProductsService } from '../services/product-service';
import { PendingVersion } from '../models/pending-version';

@Component({
  selector: 'app-approvals',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './approvals.html',
})
export class Approvals implements OnInit {
  private service = inject(ProductsService);
  protected readonly items = signal<PendingVersion[]>([]);
  protected readonly error = signal<string | null>(null);

  ngOnInit() {
    this.load();
  }

  load() {
    this.service.getPending().subscribe({
      next: (v) => this.items.set(v),
      error: () => this.error.set('Failed to load pending versions'),
    });
  }

  approve(v: PendingVersion) {
    this.service.approve(v.productId, v.versionId).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.detail ?? 'Approve failed'),
    });
  }

  reject(v: PendingVersion) {
    const reason = prompt('Reason for rejection?') ?? undefined;
    this.service.reject(v.productId, v.versionId, reason).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.detail ?? 'Reject failed'),
    });
  }
}
