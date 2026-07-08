import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { Products } from './products/products';
import { Capture } from './capture/capture';

export const routes: Routes = [
    { path: 'products', component: Products, canActivate: [MsalGuard] },
    { path: '', redirectTo: 'products', pathMatch: 'full' },
    { path: 'capture', component: Capture, canActivate: [MsalGuard] },
];