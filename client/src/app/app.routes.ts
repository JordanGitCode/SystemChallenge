import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { Products } from './products/products';
import { Capture } from './capture/capture';
import { Edit } from './edit/edit';
import { Approvals } from './approvals/approvals';
import { CategoryDemo } from './category-demo/category-demo';

export const routes: Routes = [
  { path: 'products', component: Products, canActivate: [MsalGuard] },
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'capture', component: Capture, canActivate: [MsalGuard] },
  { path: 'products/:id/edit', component: Edit, canActivate: [MsalGuard] },
  { path: 'approvals', component: Approvals, canActivate: [MsalGuard] },
  { path: 'catalog', component: CategoryDemo, canActivate: [MsalGuard] },
];
