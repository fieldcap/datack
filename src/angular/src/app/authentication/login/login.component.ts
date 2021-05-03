import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ClrLoadingState } from '@clr/angular';
import { ErrorHelper } from 'src/app/helpers/error.helper';
import { AuthenticationService } from '../authentication.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  public email: string;
  public password: string;
  public error: string;
  public loadingState: ClrLoadingState = ClrLoadingState.DEFAULT;
  public returnUrl: string;
  public duoModal: boolean;

  constructor(
    private authenticationService: AuthenticationService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  public onSubmit(): void {
    this.error = null;
    this.loadingState = ClrLoadingState.LOADING;

    this.authenticationService.login(this.email, this.password).subscribe(
      () => {
        this.router.navigate(['/']);
        this.loadingState = ClrLoadingState.SUCCESS;
      },
      (loginError) => {
        this.error = ErrorHelper.format(loginError);
        this.loadingState = ClrLoadingState.ERROR;
      }
    );
  }
}
