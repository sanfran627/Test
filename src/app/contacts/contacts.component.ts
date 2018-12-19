import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, ActivatedRouteSnapshot } from "@angular/router";
import { PayloadResponse, Contact, User } from '../../models';

@Component({
  selector: 'app-contacts',
  templateUrl: './contacts.component.html'
})
export class ContactsComponent {
  public currentPage: number = 1;
  public itemsPerPage: number = 5;
  public pageSize: number = 10;

  public contacts: Contact[] = [];
  public error: string;
  public editMode: false;
  private _route: ActivatedRoute;
  private _http: HttpClient;
  private _baseUrl: string;
  private _pos: number = 0;
  private _qty: number = 9999;
  private _user: User;
  private _opts: object;

  constructor(private route: ActivatedRoute, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this._route = route;
    this._http = http;
    this._baseUrl = baseUrl
    // should probably be using a service or whatever Angular uses for state...
    this._user = JSON.parse(localStorage.getItem('user'));
    this._opts = { headers: { Authorization: 'Bearer ' + this._user.token, Accept: 'application/json', 'Content-Type': 'application/json' } };
  }

  public onPageChange(pageNum: number): void {
    this.pageSize = this.itemsPerPage * (pageNum - 1);
  }

  ngOnInit() {
    const add: string = this.route.snapshot.queryParamMap.get('add');
    let url = this._baseUrl + `api/contacts?pos=${this._pos}&qty=${this._qty}`;

    this._http.get<PayloadResponse>(url, this._opts).subscribe(rs => {
      switch (rs.code) {
        case 0:
          this.contacts = rs.data.contacts;
          break;
        case 1:
          this.contacts = [];
          break;
        default:
          this.contacts = [];
          this.error = rs.message;
          break;
      }
    }, err => {
      console.log(err);
      this.error = err.message
    });
  }

  public update(contactId: string) {
    let c = this.contacts.find(f => {
      return f.contactId === contactId
    });
    let url = this._baseUrl + `api/contacts/${contactId}`;
    let body = JSON.stringify(c) 
    this._http.put<PayloadResponse>(url, body, this._opts).subscribe(rs => {
      if (rs.code === 0) {
        this.editMode = false;
      } else {
        //need code to revert this to the previous value if we're doing 2-way binding..
        this.editMode = false;
        this.error = rs.message;
      }
    }, err => {
      console.log(err);
      this.error = err.message
    });

  }

  public delete(contactId: string) {
    let url = this._baseUrl + `api/contacts/${contactId}`;
    this._http.delete<PayloadResponse>(url, this._opts).subscribe(rs => {
      if (rs.code === 0) {
        this.contacts = this.contacts.filter(f => {
          return f.contactId !== contactId
        });
      } else {
        this.error = rs.message;
      }
    }, err => {
      console.log(err);
      this.error = err.message
    });
  }
  public generate() {
    let url = this._baseUrl + `api/generate-contacts?qty=20`;
    this._http.get<PayloadResponse>(url, this._opts).subscribe(rs => {
        if (rs.code === 0) {
          this.ngOnInit();
        } else {
          this.error = rs.message;
        }
      }, err => {
        console.log(err);
        this.error = err.message
      });
  }
}
