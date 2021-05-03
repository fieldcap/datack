import { registerLocaleData } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import en from '@angular/common/locales/en';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { barsIcon, ClarityIcons, cogIcon, homeIcon, infoStandardIcon, successStandardIcon } from '@cds/core/icon';
import '@cds/core/icon/register.js';
import { ClarityModule } from '@clr/angular';
import { FileSizePipe, NgxFilesizeModule } from 'ngx-filesize';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ForgotPasswordComponent } from './authentication/forgot-password/forgot-password.component';
import { LoginComponent } from './authentication/login/login.component';
import { ResetPasswordComponent } from './authentication/reset-password/reset-password.component';
import { DashboardIndexComponent } from './dashboard/dashboard-index/dashboard-index.component';
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { MainLayoutComponent } from './main-layout/main-layout.component';

ClarityIcons.addIcons(homeIcon, successStandardIcon, infoStandardIcon, barsIcon, cogIcon);

registerLocaleData(en);

@NgModule({
  declarations: [
    AppComponent,
    MainLayoutComponent,
    LoginComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    DashboardIndexComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    NgxFilesizeModule,
    BrowserAnimationsModule,
    ClarityModule,
  ],
  providers: [FileSizePipe, { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }],
  bootstrap: [AppComponent],
})
export class AppModule {}
