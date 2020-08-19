import { NgModule } from '@angular/core';
import { PersonsRouting } from './persons.routing';
import { PersonsComponent } from './persons.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { ImageCropperModule } from 'ngx-image-cropper';
import { NgxFileDropModule } from 'ngx-file-drop';

@NgModule({
  declarations: [PersonsComponent],
  imports: [
    CommonModule,
    PersonsRouting,
    FormsModule,
    ReactiveFormsModule,
    ImageCropperModule,
    NgxFileDropModule
  ]
})
export class PersonsModule { }
