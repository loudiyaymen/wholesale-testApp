import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmToasterImports } from '@spartan-ng/helm/sonner';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, HlmButtonImports, HlmToasterImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './app.html',
})
export class App {
  protected readonly links = [
    { path: '/', label: 'Today', exact: true },
    { path: '/customers', label: 'Customers', exact: false },
    { path: '/products', label: 'Products', exact: false },
    { path: '/agreements', label: 'Agreements', exact: false },
    { path: '/renewals', label: 'Renewals', exact: false },
  ];
}
