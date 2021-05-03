import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ForgotPasswordComponent } from './authentication/forgot-password/forgot-password.component';
import { LoginComponent } from './authentication/login/login.component';
import { ResetPasswordComponent } from './authentication/reset-password/reset-password.component';
import { DashboardIndexComponent } from './dashboard/dashboard-index/dashboard-index.component';
import { AuthGuard } from './guards/auth.guard';
import { MainLayoutComponent } from './main-layout/main-layout.component';

const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'forgotpassword',
    component: ForgotPasswordComponent,
  },
  {
    path: 'resetpassword/:id/:token',
    component: ResetPasswordComponent,
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        component: DashboardIndexComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
