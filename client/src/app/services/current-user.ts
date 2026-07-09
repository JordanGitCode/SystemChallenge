import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { apiBaseUrl } from '../auth-config';
import { Me } from '../models/me';

@Injectable({ providedIn: 'root' })
export class CurrentUser {
  private http = inject(HttpClient);

  readonly name = signal<string | null>(null);
  readonly isManager = signal(false);

  load() {
    this.http.get<Me>(`${apiBaseUrl}/me`).subscribe({
      next: (me) => {
        this.name.set(me.name);
        this.isManager.set(me.roles?.includes('Manager') ?? false);
      },
      error: () => this.isManager.set(false),
    });
  }
}
