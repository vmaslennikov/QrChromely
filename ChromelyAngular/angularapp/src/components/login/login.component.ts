import { Component, OnInit, NgZone } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ChromelyService } from '../../services/chromely.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styles: []
})
export class LoginComponent implements OnInit {

  constructor(
    private router: Router,
    private _chromelyService: ChromelyService,
    private _zone: NgZone) {
  }

  loginForm = new FormGroup({});

  message: string;

  ngOnInit() {
    this.loginForm = new FormGroup({
      username: new FormControl('', [Validators.required, Validators.minLength(3)]),
      password: new FormControl('', [Validators.required, Validators.minLength(3)]),
    });
  }

  Login() {
    //alert(JSON.stringify(this.loginForm.value));
    this.message = null;

    this._chromelyService.cefQueryPostRequest(
      '/auth/login',
      null,
      this.loginForm.value,
      data => {
        this._zone.run(() => {
          //alert(JSON.stringify(data));
          if (data && data.Result) {
            this.router.navigate(['/persons', {}]);
          } else {
            this.message = 'Неверные логин или пароль';
          }
        });
      });

  }
}
