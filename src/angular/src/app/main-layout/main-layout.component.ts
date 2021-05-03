import { DOCUMENT } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../authentication/authentication.service';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss'],
})
export class MainLayoutComponent implements OnInit {
  public static themes = [
    { name: 'Light', hrefs: ['clr-ui.min.css', 'assets/styles-light.css'] },
    { name: 'Dark', hrefs: ['clr-ui-dark.min.css', 'assets/styles-dark.css'] },
  ];

  public isMenuOpen = false;

  public linkRef: HTMLLinkElement;

  constructor(
    @Inject(DOCUMENT) private document: Document,
    private router: Router,
    private authenticationService: AuthenticationService
  ) {}

  ngOnInit(): void {}

  public logout(): void {
    this.authenticationService.logout().subscribe(
      () => {
        this.router.navigate(['/login']);
      },
      () => {
        this.router.navigate(['/login']);
      }
    );
  }

  public setTheme(index: number) {
    localStorage.setItem('theme', index.toString());

    const theme = MainLayoutComponent.themes[index];

    for (let i = 0; i < 2; i++) {
      this.linkRef = this.document.getElementsByTagName('link')[i + 1];
      this.linkRef.href = theme.hrefs[i];
    }
  }
}
