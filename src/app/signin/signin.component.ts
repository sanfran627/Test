import { Component, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Response } from '@angular/http';
import { PayloadResponse } from '../../models';

@Component({
  selector: 'login',
  templateUrl: 'signin.component.html'
})
export class SigninComponent {
  loading = false;
  submitted = false;
  private _http: HttpClient;
  private _baseUrl: string;

  constructor(
    public router: Router,
    public http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this._http = http;
    this._baseUrl = baseUrl
  }

  login(event, username, password) {
    this.submitted = true;
    event.preventDefault();
    let body = JSON.stringify({ action: 'Signin', signin: { username, password } });

    this.loading = true;
    this.http.post(this._baseUrl + 'api/signin', body, { headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' } })
      .subscribe(
      (rs: PayloadResponse) => {
          if (rs.code === 0) {
            localStorage.setItem('user', JSON.stringify(rs.data.user));
            this.router.navigate(['contacts']);
          } else {
            alert(rs.message);
          }
          this.loading = false;
        },
        error => {
          this.loading = false;
          alert(error.text());
          console.log(error.text());
        }
      );
  }
}
