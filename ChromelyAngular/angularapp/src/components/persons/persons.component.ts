import { Component, OnInit, NgZone } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ChromelyService } from '../../services/chromely.service';
import { ImageCroppedEvent } from 'ngx-image-cropper';
import { NgxFileDropEntry, FileSystemFileEntry } from 'ngx-file-drop';

@Component({
  selector: 'app-persons',
  templateUrl: './persons.component.html',
  styleUrls: ['./persons.component.css'],
})
export class PersonsComponent implements OnInit {

  persons: any[];

  zones: any[];
  positions: any[];
  blockreasons: any[];
  statuses = ['активна', 'заблокирована'];

  errors: string[];
  personForm: FormGroup;
  selectedPerson: any;
  selectedAll = false;
  personFromEnabled = false;


  constructor(
    private router: Router,
    private _chromelyService: ChromelyService,
    private _zone: NgZone) {
  }

  ngOnInit() {
    this.persons = [];
    this.errors = [];

    this.personForm = new FormGroup({
      profilePhoto: new FormControl(''),
      fullname: new FormControl('', [Validators.required]),
      company: new FormControl('', [Validators.required]),
      position: new FormControl('', [Validators.required]),
      zone: new FormControl('', [Validators.required]),
      email: new FormControl(''),
      phone: new FormControl(''),
      hasdeclaration: new FormControl(''),
      haspcr: new FormControl(''),
      status: new FormControl('', [Validators.required]),
      blockreason: new FormControl(''),
      deleted: new FormControl(''),
    });
    this.GetDictionaries();
    this.GetPersons();

    this.personForm.disable();
    this.personForm.reset();
  }

  imageChangedEvent: any = '';
  croppedImage: any = '';

  fileChangeEvent(event: any): void {
    this.imageChangedEvent = event;
  }
  imageCropped(event: ImageCroppedEvent) {
    this.croppedImage = event.base64;
  }
  imageLoaded() {
    // show cropper
  }
  cropperReady() {
    // cropper ready
  }
  loadImageFailed() {
    // show message
  }

  selectAll() {
    this.persons.forEach((o) => {
      o.selected = !this.selectedAll;
    });
    this.selectedAll = !this.selectedAll;
  }

  AddPerson() {
    this.personForm.reset();
    this.personForm.enable();
    this.croppedImage = null;
    this.selectedPerson = null;
    this.personFromEnabled = true;
    this.imageChangedEvent = '';
  }
  EditPerson() {
    var selected = [];
    this.persons.forEach((o) => {
      if (o.selected) {
        selected.push(o);
      }
    });
    if (selected.length == 1) {
      this.personForm.reset();
      this.personForm.enable();
      this.selectedPerson = selected[0];
      if (this.selectedPerson.Photo) {
        this.croppedImage = this.selectedPerson.Photo;
      }
      this.personForm.controls['profilePhoto'].setValue(null);
      this.personForm.controls['fullname'].setValue(this.selectedPerson.FullName);
      this.personForm.controls['company'].setValue(this.selectedPerson.Company);
      this.personForm.controls['position'].setValue(this.selectedPerson.Position);
      this.personForm.controls['zone'].setValue(this.selectedPerson.Zone);
      this.personForm.controls['email'].setValue(this.selectedPerson.Email);
      this.personForm.controls['hasdeclaration'].setValue(this.selectedPerson.HasDeclaration);
      this.personForm.controls['haspcr'].setValue(this.selectedPerson.HasPcr);
      this.personForm.controls['status'].setValue(this.selectedPerson.Status);
      this.personForm.controls['blockreason'].setValue(this.selectedPerson.BlockReason);
      this.personForm.controls['deleted'].setValue(this.selectedPerson.Deleted);
      this.personFromEnabled = true;
    } else {
      alert('Необходимо выбрать только 1 запись для редактирования.')
    }
  }

  DeletePerson() {
    var ids = [];
    this.persons.forEach((o) => {
      if (o.selected) {
        ids.push(o.Id);
      }
    });
    if (ids.length > 0 && confirm(`Записи (${ids.length}) будут помечены удаленными. Вы уверены?`)) {
      var datajson = { ids: ids };
      this._chromelyService.cefQueryPostRequest(
        '/persons/delete',
        null,
        datajson,
        data => {
          this._zone.run(() => {
            //alert(JSON.stringify(data));
            if (data && data.Status == "ok") {
              // тут надо грид обновить
              this.GetPersons();
            } else {
              this.errors = data.Errors;
            }
          });
        });
    }
  }


  GetDictionaries() {
    this._chromelyService.cefQueryGetRequest(
      '/data/get',
      null,
      data => {
        this._zone.run(() => {
          //alert(JSON.stringify(data));
          if (data && data.Status == "ok") {
            this.zones = data.Result.Zones;
            this.positions = data.Result.Positions;
            this.blockreasons = data.Result.BlockReasons;
          } else {
            this.errors = data.Errors;
          }
        });
      });
  }

  GetPersons() {
    this.persons = [];
    this._chromelyService.cefQueryGetRequest(
      '/persons/all',
      null,
      data => {
        this._zone.run(() => {
          //alert(JSON.stringify(data));
          if (data && data.Status == "ok") {
            this.persons = data.Result;
          } else {
            this.errors = data.Errors;
          }
        });
      });
  }

  SavePerson() {
    this.errors = [];

    if (!this.croppedImage) {
      this.errors.push('Фото обязательно');
    }
    if (this.personForm.value.status !== "активна") {
      this.errors.push('Выберите причину статуса');
    }
    if (this.errors.length > 0) {
      return;
    }
    var copy = Object.assign(
      {
        id: this.selectedPerson ? this.selectedPerson.Id : null,
        photo: this.croppedImage
      },
      this.personForm.value);
    copy.deleted = copy.deleted ? true : false;
    this._chromelyService.cefQueryPostRequest(
      '/persons/modify',
      null,
      copy,
      data => {
        this._zone.run(() => {
          //alert(JSON.stringify(data));
          if (data && data.Status == "ok") {
            if (copy.id) {
              // тут надо грид обновить
              this.GetPersons();
            }
            else {
              this.persons.push(data.Result);
            }
            this.personForm.disable();
            this.personForm.reset();
            this.personFromEnabled = false;
          } else {
            this.errors = data.Errors;
          }
        });
      });
  }

  //droppedImage: any = '';
  //isFileSelected = false;

  //readImageFile(file: any) {
  //  const reader = new FileReader();
  //  reader.onload = (e: any) => {
  //    this.croppedImage = e.target.result;
  //  };
  //  reader.readAsDataURL(file);
  //}

  //allowDrop(event: any) {
  //  event.preventDefault();
  //}

  ////drag and drop image select
  //dropHandler(event: any) {
  //  event.preventDefault();
  //  if (!this.isFileSelected && event.dataTransfer.items.length > 0 &&
  //    event.dataTransfer.items[0].kind === 'file' &&
  //    event.dataTransfer.items[0].type.indexOf('image') > -1) {
  //    this.isFileSelected = true;
  //    this.readImageFile(event.dataTransfer.items[0].getAsFile());
  //  } else {
  //    return false;
  //  }
  //}

  ////public dropped(event: any) {
  ////  event.files[0].fileEntry.file(
  ////    (ev) => {
  ////      this.imageChangedEvent = { target: { files: [ev] } }
  ////    },
  ////    () => console.log('Failed to load image')
  ////  );
  ////}
  //public dropped(files: NgxFileDropEntry[]) {
  //  for (const droppedFile of files) {

  //    // Is it a file?
  //    if (droppedFile.fileEntry.isFile) {
  //      const fileEntry = droppedFile.fileEntry as FileSystemFileEntry;
  //      fileEntry.file(
  //        (ev) => {
  //          this.imageChangedEvent = { target: { files: [ev] } }
  //        }
  //      );
  //    }

  //  }
  //}
}
