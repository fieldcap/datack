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
import { ServersIndexComponent } from './servers/servers-index/servers-index.component';

const logoIcon = `<svg xmlns="http://www.w3.org/2000/svg" class="fl-svgdocument" viewBox="48 192.38 127.25 127.25">
<title>Logo</title>
 <g>
  <circle transform="matrix(0.7198602446673189, 0, 0, 0.7198602446673189, 48.00000000000002, 192.37515622938943)" fill="#179bd3" r="88.385" cy="88.385" cx="88.385" id="_BATRyMAZJLXPz1LAsjwes"/>
  <path fill="#ffffff" d="m160.46841,243.73467c-2.31887,0 -4.19782,-1.87894 -4.19782,-4.19662c0,-0.66168 0.16691,-1.27926 0.43993,-1.83602c-4.5531,-10.8695 -12.96304,-19.72413 -23.52136,-24.84115l0,14.08611l-8.39086,8.39325l2.79934,4.19782l18.18259,0l0,10.49037l14.19699,0c-0.08584,-0.5985 -0.13949,-1.20534 -0.23844,-1.79906l4.12032,-0.76898c0.4912,2.88756 0.80356,5.83474 0.80356,8.86179c0,28.96982 -23.48321,52.45304 -52.45184,52.45304c-23.19708,0 -42.82584,-15.07685 -49.74788,-35.95267c-1.5797,-0.60088 -2.70754,-2.11858 -2.70754,-3.9093c0,-2.31768 1.87894,-4.19782 4.19662,-4.19782c2.31649,0 4.19662,1.87894 4.19662,4.19782c0,1.2876 -0.59134,2.4214 -1.50339,3.19277c0.92993,2.68608 2.115,5.25293 3.48129,7.70057l12.7079,-10.89334l0,-14.68819l12.58987,0l0,-8.39444l-8.39325,0l0,-8.39206l-19.24009,0c-2.46909,5.79897 -3.84015,12.18094 -3.84015,18.88362c0,1.13142 0.09299,2.24019 0.1693,3.35253l-4.15728,0.77375c-0.1073,-1.36629 -0.20864,-2.73496 -0.20864,-4.12986c0,-28.96863 23.48441,-52.45423 52.45542,-52.45423c21.5077,0 39.93112,12.97138 48.02512,31.49495c0.07749,-0.00358 0.15141,-0.02265 0.23248,-0.02265c2.31649,0 4.19424,1.87894 4.19424,4.19782c0,2.32126 -1.87656,4.2002 -4.19305,4.2002zm-18.88481,0l-12.59107,0l0,12.58868l12.59107,0l0,-12.58868zm18.88481,12.58868c0,-0.70818 -0.06915,-1.39848 -0.10134,-2.09712l-14.58804,0l0,6.29374l-8.39444,0l-0.02146,11.22955l-3.57071,3.96414l14.39728,12.69121c7.61234,-8.53035 12.2787,-19.74679 12.2787,-32.08152zm-87.90378,27.38774c8.69727,12.59226 23.18039,20.87225 39.64499,20.87225c12.7699,0 24.34996,-4.99541 32.98046,-13.09538l-14.08015,-12.58868l-4.21332,4.70093l-10.49037,0l0,4.19782l-20.98193,0l0,-6.29494l-9.4424,-9.4424l-1.04916,1.05035l-12.36812,10.60005zm39.64499,-0.11088l0,-12.58868l-12.58987,0l0,12.58868l12.58987,0zm-25.17855,-25.17975l0,8.1095l8.39325,8.39206l0,-8.10712l20.98193,0l0,12.58987l8.39206,0l8.39086,-8.39206l0,-10.49395l-8.39086,0l0,-16.78411l-4.19543,-6.29374l-12.58987,0l0,8.39206l-8.39325,0l0,12.58749l-12.58868,0zm4.19543,-20.97955l0,4.19543l12.58987,0l0,-12.58987l-4.19782,0l-8.39325,0l0.00119,8.39444l0,0zm4.19782,-26.31474c-10.9601,4.07383 -20.06273,11.96634 -25.60179,22.11812l17.20854,0l0,-8.39444l0,0l8.39325,0l0,-13.72368zm33.56823,-0.02623c-5.2267,-1.9469 -10.87307,-3.03301 -16.78292,-3.03301c-4.36473,0 -8.57089,0.63188 -12.59107,1.71442l0,15.0685l8.39325,0l0,8.39444l12.59107,0l8.38967,-8.39444l0,-13.74991z" id="_Xc3C9J8BZYW5QKfviSALD"/>
 </g>
</svg>`;

ClarityIcons.addIcons(['logo', logoIcon], homeIcon, successStandardIcon, infoStandardIcon, barsIcon, cogIcon);

registerLocaleData(en);

@NgModule({
  declarations: [
    AppComponent,
    MainLayoutComponent,
    LoginComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    DashboardIndexComponent,
    ServersIndexComponent,
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
