import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ClrLoadingState } from '@clr/angular';
import { ErrorHelper } from 'src/app/helpers/error.helper';
import { AuthenticationService } from '../authentication.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss'],
})
export class ForgotPasswordComponent implements OnInit {
  public email: string;
  public password: string;
  public error: string;
  public success: boolean;
  public loadingState: ClrLoadingState = ClrLoadingState.DEFAULT;
  public returnUrl: string;

  constructor(private authenticationService: AuthenticationService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit() {
    this.authenticationService.logout();
  }

  public onSubmit(): void {
    this.error = null;
    this.loadingState = ClrLoadingState.LOADING;
    this.success = false;

    this.authenticationService.resetPasswordRequest(this.email).subscribe(
      () => {
        this.success = true;
        this.loadingState = ClrLoadingState.SUCCESS;
      },
      (error: HttpErrorResponse) => {
        this.error = ErrorHelper.format(error);
        this.loadingState = ClrLoadingState.ERROR;
      }
    );
  }
}
