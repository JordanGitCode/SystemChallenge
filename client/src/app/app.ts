import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { MsalService } from '@azure/msal-angular';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  private readonly msal = inject(MsalService);
  protected readonly isLoggedIn = signal(false);

  async ngOnInit() {
    
    await this.msal.instance.initialize();
    const result = await this.msal.instance.handleRedirectPromise();
    
    if (result?.account) 
      this.msal.instance.setActiveAccount(result.account);
    
    this.isLoggedIn.set(this.msal.instance.getAllAccounts().length > 0);
  }

  login()
  { 
    this.msal.loginRedirect();
  }
  
  logout() 
  { 
    this.msal.logoutRedirect(); 
  }
}