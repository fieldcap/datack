import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ForgotPasswordComponent } from './authentication/forgot-password/forgot-password.component';
import { LoginComponent } from './authentication/login/login.component';
import { ResetPasswordComponent } from './authentication/reset-password/reset-password.component';
import { DashboardIndexComponent } from './dashboard/dashboard-index/dashboard-index.component';
import { MainLayoutComponent } from './main-layout/main-layout.component';
import { ServersIndexComponent } from './servers/servers-index/servers-index.component';

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
    //canActivate: [AuthGuard],
    children: [
      {
        path: '',
        component: DashboardIndexComponent,
      },
      {
        path: 'servers',
        component: ServersIndexComponent,
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
