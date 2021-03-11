import { HttpClient, HttpParams } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { MsalService } from "@azure/msal-angular";
import { AuthenticationResult } from "@azure/msal-browser";
import * as MicrosoftGraph from "@microsoft/microsoft-graph-types";


@Component({
  selector: "app-root",
  templateUrl: "app.component.html",
  styleUrls: [] ,
})
export class AppComponent implements OnInit {
  loggedIn = false;
  users?: MicrosoftGraph.User[];
  friends: MicrosoftGraph.User[] = [];
  userNameFilter: string = '';

  constructor(private authService: MsalService, private client: HttpClient) {}

  ngOnInit() {
    this.checkAccount();
    if(this.loggedIn) {
      this.getFriends();
    }
  }

  checkAccount() {
    this.loggedIn = this.authService.instance.getAllAccounts().length > 0;
  }

  login() {
    this.authService
        .loginPopup()
        .subscribe((response: AuthenticationResult) => {
          this.authService.instance.setActiveAccount(response.account);
          this.checkAccount();
          this.getFriends();
        });
  }

  logout() {
      this.authService.logout();
  }

  getUsers() {
    let params = new HttpParams().set("$top", "10");
    if (this.userNameFilter) {
      params = params.set(
        "$filter",
        `startsWith(displayName, '${this.userNameFilter}')`
      );
    }
    let url = `https://graph.microsoft.com/v1.0/users?${params.toString()}`;
    this.client
      .get<any>(url)
      .subscribe((users) => (this.users = users.value));
  }

  addFriendToList(id: string) {
    let params = new HttpParams().set(
          "$filter",
          `id eq '${id}'`
      );
    let url = `https://graph.microsoft.com/v1.0/users?${params.toString()}`;
    this.client
        .get<any>(url)
        .subscribe(user => {
          this.friends?.push(user.value[0]);
        });
  }

  getFriends() {
    this.client.get<any>("http://localhost:5000/api/Friends/getAll")
        .subscribe(result => {
          this.friends = [];
          for(let u of result) {
            this.addFriendToList(u);
          }
        });
  }

  addFriend(u: MicrosoftGraph.User) {
    this.client.post<any>(`http://localhost:5000/api/Friends/add?friendId=${u.id}`, null)
        .subscribe(result => this.getFriends());

  }

  removeFriend(u: MicrosoftGraph.User) {
    this.client.delete<any>(`http://localhost:5000/api/Friends/remove?friendId=${u.id}`)
        .subscribe(result => this.getFriends());
  }
}
