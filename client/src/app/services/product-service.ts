import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiBaseUrl } from '../auth-config';
import { ProductResponse } from '../models/product';
import { CreateProductRequest } from '../models/create-product-request';

@Injectable({ providedIn: 'root' })
export class ProductsService {

    private http = inject(HttpClient);
    
    getAll(): Observable<ProductResponse[]> {
        return this.http.get<ProductResponse[]>(`${apiBaseUrl}/product`);
    }

    create(req: CreateProductRequest): Observable<ProductResponse> {
        return this.http.post<ProductResponse>(`${apiBaseUrl}/product/capture`, req);
    }
}