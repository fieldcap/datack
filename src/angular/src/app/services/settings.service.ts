import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Setting } from '../models/setting.model';

@Injectable({
  providedIn: 'root',
})
export class SettingsService {
  constructor(private http: HttpClient) {}

  public get(): Observable<Setting[]> {
    return this.http.get<Setting[]>(`/Api/Settings`);
  }

  public update(settings: Setting[]): Observable<void> {
    return this.http.put<void>(`/Api/Settings`, { settings });
  }
}
