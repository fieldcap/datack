import { DOCUMENT } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MainLayoutComponent } from './components/main-layout/main-layout.component';

@Component({
  selector: 'app-root',
  template: '<router-outlet></router-outlet>',
})
export class AppComponent {
  public linkRef: HTMLLinkElement;

  constructor(@Inject(DOCUMENT) private document: Document) {
    const stored = parseInt(localStorage.getItem('theme')) || 0;
    let theme = MainLayoutComponent.themes[stored];

    for (let i = 0; i < 2; i++) {
      this.linkRef = this.document.getElementsByTagName('link')[i + 1];
      this.linkRef.href = theme.hrefs[i];
    }
  }
}
