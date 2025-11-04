import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BusyService {
  busyRequestCount = signal(0);

  busy() {
    this.busyRequestCount.update((currrent) => currrent + 1);
  }
  idle() {
    this.busyRequestCount.update((current) => Math.max(0, current - 1));
  }
}
