import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RequestsRoutingModule } from './requests-routing.module';
import { RequestsComponent } from './requests.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [RequestsComponent],
  imports: [
    CommonModule,
    RequestsRoutingModule,
    FormsModule,
    ReactiveFormsModule,
  ]
})
export class RequestsModule { }
