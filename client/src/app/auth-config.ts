import { PublicClientApplication, InteractionType, IPublicClientApplication, LogLevel } from '@azure/msal-browser';
import { MsalGuardConfiguration, MsalInterceptorConfiguration } from '@azure/msal-angular';

const tenantId  = 'b19dc195-e548-4aa9-9997-fad8cac0bff4';
const clientId  = '719d39a4-6344-4e2a-a87e-b7f59cb15a69';
const apiScope  = 'api://e2ce9829-ab51-4f14-bfa6-7477859c5b39/user-access';

// Must match the profile you actually run the API on (dotnet run https = 7089)
export const apiBaseUrl = 'https://localhost:7089';

export function MSALInstanceFactory(): IPublicClientApplication {
    return new PublicClientApplication({
        auth: {
        clientId,
        authority: `https://login.microsoftonline.com/${tenantId}`,
        redirectUri: 'http://localhost:4200',
        postLogoutRedirectUri: 'http://localhost:4200',
        },
        cache: { cacheLocation: 'localStorage' },
    });
}

export function MSALGuardConfigFactory(): MsalGuardConfiguration {
    return { interactionType: InteractionType.Redirect, authRequest: { scopes: [apiScope] } };
}

export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
    const map = new Map<string, Array<string>>([
        [`${apiBaseUrl}/product`,    [apiScope]],
        [`${apiBaseUrl}/product/*`,  [apiScope]],
        [`${apiBaseUrl}/catalog`,    [apiScope]],
        [`${apiBaseUrl}/catalog/*`,  [apiScope]],
    ]);

    return { interactionType: InteractionType.Redirect, protectedResourceMap: map };
}