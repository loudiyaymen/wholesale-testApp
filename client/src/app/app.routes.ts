import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', title: 'Today', loadComponent: () => import('./pages/today/today').then((m) => m.Today) },
  { path: 'customers', title: 'Customers', loadComponent: () => import('./pages/customers/customer-list').then((m) => m.CustomerList) },
  { path: 'customers/new', title: 'New customer', loadComponent: () => import('./pages/customers/customer-form').then((m) => m.CustomerForm) },
  { path: 'customers/:id', title: 'Customer', loadComponent: () => import('./pages/customers/customer-detail').then((m) => m.CustomerDetail) },
  { path: 'customers/:id/edit', title: 'Edit customer', loadComponent: () => import('./pages/customers/customer-form').then((m) => m.CustomerForm) },
  { path: 'products', title: 'Products', loadComponent: () => import('./pages/products/product-list').then((m) => m.ProductList) },
  { path: 'products/new', title: 'New product', loadComponent: () => import('./pages/products/product-form').then((m) => m.ProductForm) },
  { path: 'products/:id/edit', title: 'Edit product', loadComponent: () => import('./pages/products/product-form').then((m) => m.ProductForm) },
  { path: 'agreements', title: 'Agreements', loadComponent: () => import('./pages/agreements/agreement-list').then((m) => m.AgreementList) },
  { path: 'agreements/new', title: 'New quote', loadComponent: () => import('./pages/agreements/quote-form').then((m) => m.QuoteForm) },
  { path: 'agreements/:id', title: 'Agreement', loadComponent: () => import('./pages/agreements/agreement-detail').then((m) => m.AgreementDetail) },
  { path: 'renewals', title: 'Renewals', loadComponent: () => import('./pages/renewals/renewals').then((m) => m.Renewals) },
  { path: '**', redirectTo: '' }, // TODO: proper 404 page
];
