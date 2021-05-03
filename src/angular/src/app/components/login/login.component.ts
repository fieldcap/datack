import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  public userName: string;
  public password: string;
  public error: string;
  public loggingIn: boolean;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {}

  public login(): void {
    this.error = null;
    this.loggingIn = true;
    this.authService.login(this.userName, this.password).subscribe(
      () => {
        this.router.navigate(['/']);
      },
      (err) => {
        this.loggingIn = false;
        this.error = err.error;
      }
    );
  }
}
