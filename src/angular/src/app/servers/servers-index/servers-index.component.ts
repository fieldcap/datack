import { Component, HostBinding, OnInit } from '@angular/core';

@Component({
  selector: 'app-servers-index',
  templateUrl: './servers-index.component.html',
  styleUrls: ['./servers-index.component.scss'],
})
export class ServersIndexComponent implements OnInit {
  @HostBinding('class.content-container') class = true;

  constructor() {}

  ngOnInit(): void {}
}
