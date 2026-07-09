import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { CurrentUser } from './services/current-user';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  private readonly msal = inject(MsalService);
  private readonly user = inject(CurrentUser);

  protected readonly isLoggedIn = signal(false);
  protected readonly isManager = this.user.isManager;

  async ngOnInit() {
    await this.msal.instance.initialize();
    const result = await this.msal.instance.handleRedirectPromise();

    if (result?.account) {
      this.msal.instance.setActiveAccount(result.account);
    }

    const loggedIn = this.msal.instance.getAllAccounts().length > 0;
    this.isLoggedIn.set(loggedIn);

    if (loggedIn) {
      this.user.load();
    }
  }

  login() {
    this.msal.loginRedirect();
  }

  logout() {
    this.msal.logoutRedirect();
  }
}
