import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiBaseUrl } from '../auth-config';
import { ProductResponse } from '../models/product';
import { CreateProductRequest } from '../models/create-product-request';
import { PendingVersion } from '../models/pending-version';
import { ProductReadModel } from '../models/product-read';

@Injectable({ providedIn: 'root' })
export class ProductsService {
  private http = inject(HttpClient);

  getCatalog(): Observable<ProductReadModel[]> {
    return this.http.get<ProductReadModel[]>(`${apiBaseUrl}/catalog`);
  }

  getAll(): Observable<ProductResponse[]> {
    return this.http.get<ProductResponse[]>(`${apiBaseUrl}/product`);
  }

  getById(id: string): Observable<ProductResponse> {
    return this.http.get<ProductResponse>(`${apiBaseUrl}/product/${id}`);
  }

  update(id: string, req: CreateProductRequest): Observable<ProductResponse> {
    return this.http.post<ProductResponse>(`${apiBaseUrl}/product/update/${id}`, req);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${apiBaseUrl}/product/${id}`);
  }

  create(req: CreateProductRequest): Observable<ProductResponse> {
    return this.http.post<ProductResponse>(`${apiBaseUrl}/product/capture`, req);
  }

  submitForReview(productId: string, versionId: string): Observable<ProductResponse> {
    return this.http.post<ProductResponse>(`${apiBaseUrl}/product/submit`, {
      productId,
      versionId,
    });
  }

  getPending(): Observable<PendingVersion[]> {
    return this.http.get<PendingVersion[]>(`${apiBaseUrl}/product/pending`);
  }

  approve(
    productId: string,
    versionId: string,
    decisionReason?: string,
  ): Observable<ProductResponse> {
    return this.http.post<ProductResponse>(`${apiBaseUrl}/product/approve`, {
      productId,
      versionId,
      decisionReason,
    });
  }

  reject(
    productId: string,
    versionId: string,
    decisionReason?: string,
  ): Observable<ProductResponse> {
    return this.http.post<ProductResponse>(`${apiBaseUrl}/product/reject`, {
      productId,
      versionId,
      decisionReason,
    });
  }
}
