import { Component, OnInit, NgZone } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ChromelyService } from '../../services/chromely.service';

@Component({
  selector: 'app-requests',
  templateUrl: './requests.component.html',
  styles: []
})
export class RequestsComponent implements OnInit {

  requests: any[];

  events: any[];

  errors: string[];
  requestForm: FormGroup;

  selectedRequest: any;
  selectedAll = false;


  constructor(
    private router: Router,
    private _chromelyService: ChromelyService,
    private _zone: NgZone) {
  }

  ngOnInit() {
    this.requests = [];
    this.errors = [];

    this.requestForm = new FormGroup({
      event: new FormControl('', [Validators.required]),
      deleted: new FormControl(''),
    });
    //this.GetDictionaries();
    //this.GetPersons();

    this.requestForm.disable();
    this.requestForm.reset();
  }

  selectAll() {
    this.requests.forEach((o) => {
      o.selected = !this.selectedAll;
    });
    this.selectedAll = !this.selectedAll;
  }

  AddRequest() {
    this.requestForm.reset();
    this.requestForm.enable();
    this.selectedRequest = null;
  }
  EditRequest() {
    var selected = [];
    this.requests.forEach((o) => {
      if (o.selected) {
        selected.push(o);
      }
    });
    if (selected.length == 1) {
      this.requestForm.reset();
      this.requestForm.enable();
      this.selectedRequest = selected[0];

      this.requestForm.controls['event'].setValue(this.selectedRequest.Event);
      this.requestForm.controls['deleted'].setValue(this.selectedRequest.Deleted);
    } else {
      alert('Необходимо выбрать только 1 запись для редактирования.')
    }
  }

  DeleteRequest() {
    var ids = [];
    this.requests.forEach((o) => {
      if (o.selected) {
        ids.push(o.Id);
      }
    });
    if (ids.length > 0 && confirm(`Записи (${ids.length}) будут помечены удаленными. Вы уверены?`)) {
      var datajson = { ids: ids };
      this._chromelyService.cefQueryPostRequest(
        '/requests/delete',
        null,
        datajson,
        data => {
          this._zone.run(() => {
            //alert(JSON.stringify(data));
            if (data && data.Status == "ok") {
              // тут надо грид обновить
              this.GetRequests();
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
            this.events = data.Result.Events;
          } else {
            this.errors = data.Errors;
          }
        });
      });
  }

  GetRequests() {
    this.requests = [];
    this._chromelyService.cefQueryGetRequest(
      '/requests/all',
      null,
      data => {
        this._zone.run(() => {
          //alert(JSON.stringify(data));
          if (data && data.Status == "ok") {
            this.requests = data.Result;
          } else {
            this.errors = data.Errors;
          }
        });
      });
  }

  SaveRequest() {
    this.errors = [];

    var copy = Object.assign(
      { id: this.selectedRequest ? this.selectedRequest.Id : null },
      this.requestForm.value);
    copy.deleted = copy.deleted ? true : false;
    this._chromelyService.cefQueryPostRequest(
      '/requests/modify',
      null,
      copy,
      data => {
        this._zone.run(() => {
          //alert(JSON.stringify(data));
          if (data && data.Status == "ok") {
            if (copy.id) {
              // тут надо грид обновить
              this.GetRequests();
            }
            else {
              this.requests.push(data.Result);
            }
            this.requestForm.disable();
            this.requestForm.reset();
          } else {
            this.errors = data.Errors;
          }
        });
      });
  }

}
