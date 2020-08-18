import { NgModule } from '@angular/core';
import { PersonsRouting } from './persons.routing';
import { PersonsComponent } from './persons.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';



@NgModule({
  declarations: [PersonsComponent],
  imports: [
    CommonModule,
    PersonsRouting,
    FormsModule,
    ReactiveFormsModule
  ]
})
export class PersonsModule { }
