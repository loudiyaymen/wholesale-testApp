import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Renewal } from '../core/models';
import { DockBoard } from './dock-board';

const renewal = (days: number, id = String(days)): Renewal => ({
  agreementId: id,
  customerId: 'c1',
  customerName: 'Bayview Café Group',
  contactEmail: 'x@y.example',
  productId: 'p1',
  sku: 'COF-1',
  productName: 'Coffee',
  unitPrice: 21,
  annualVolume: 2400,
  endDate: '2026-11-05',
  daysRemaining: days,
  annualValue: 50400,
});

describe('DockBoard', () => {
  async function render(list: Renewal[]) {
    TestBed.configureTestingModule({ imports: [DockBoard], providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(DockBoard);
    fixture.componentRef.setInput('renewals', list);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('shows a row per renewal with the day count', async () => {
    const el = await render([renewal(8), renewal(60)]);

    const days = [...el.querySelectorAll('[data-testid=days]')].map((d) => d.textContent?.trim());
    expect(days).toEqual(['8', '60']);
  });

  it('marks only the urgent ones (14 days or less) with the yellow bar', async () => {
    const el = await render([renewal(14), renewal(15)]);

    const rows = el.querySelectorAll('li');
    expect(rows[0].querySelector('.bg-signal')).not.toBeNull();
    expect(rows[1].querySelector('.bg-signal')).toBeNull();
  });

  it('renewal quote starts the day after the current one ends', async () => {
    const el = await render([renewal(8)]);

    const href = el.querySelector('a[href*="agreements/new"]')?.getAttribute('href') ?? '';
    expect(href).toContain('startDate=2026-11-06');
    expect(href).toContain('productId=p1');
  });

  it('says "day left" for exactly one day', async () => {
    const el = await render([renewal(1)]);
    expect(el.textContent).toContain('day left');
    expect(el.textContent).not.toContain('days left');
  });
});
