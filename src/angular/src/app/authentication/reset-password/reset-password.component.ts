import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ClrLoadingState } from '@clr/angular';
import { AuthenticationService } from 'src/app/authentication/authentication.service';
import { ErrorHelper } from 'src/app/helpers/error.helper';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss'],
})
export class ResetPasswordComponent implements OnInit {
  public password1: string;
  public password2: string;
  public error: string;
  public loadingState: ClrLoadingState = ClrLoadingState.DEFAULT;
  public token: string;
  public userId: string;

  constructor(
    private authenticationService: AuthenticationService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.userId = params['id'];
      this.token = params['token'];
    });
  }

  public onSubmit(): void {
    this.error = null;
    this.loadingState = ClrLoadingState.LOADING;

    this.authenticationService.resetPassword(this.userId, this.token, this.password1, this.password2).subscribe(
      () => {
        this.router.navigate(['/']);
      },
      (error: HttpErrorResponse) => {
        this.error = ErrorHelper.format(error);
        this.loadingState = ClrLoadingState.ERROR;
      }
    );
  }
}
