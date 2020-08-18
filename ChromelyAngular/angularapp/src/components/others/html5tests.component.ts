import { Component, OnInit, Input  } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-html5tests',
  template: '<iframe [src]="_urlSafe" width="100%" height="800"  frameborder="0" allowFullscreen></iframe>'
})
export class Html5TestsComponent implements OnInit {
  _url: string = "https://html5test.com/";
  _urlSafe: SafeResourceUrl;

  constructor(public sanitizer: DomSanitizer) { }

  ngOnInit() {
    this._urlSafe= this.sanitizer.bypassSecurityTrustResourceUrl(this._url);
  }
}
