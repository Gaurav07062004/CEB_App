using Microsoft.Identity.Client;

namespace MyEmploylog.Services;

public record UserInfo(string Name, string Email);

/// <summary>
/// Microsoft sign-in (authorization code + PKCE) using MSAL.
/// The user types the password on Microsoft's own page, shown inside the app.
/// </summary>
public class AuthService
{
    static readonly string[] Scopes = ["User.Read"];

    readonly IPublicClientApplication _pca;

    public AuthService()
    {
        var builder = PublicClientApplicationBuilder.Create(AppConfig.ClientId)
            .WithAuthority($"https://login.microsoftonline.com/{AppConfig.TenantId}");

#if ANDROID
        builder = builder.WithRedirectUri($"msal{AppConfig.ClientId}://auth");
#elif IOS
        builder = builder
            .WithRedirectUri($"msauth.{AppConfig.BundleId}://auth")
            .WithIosKeychainSecurityGroup("com.microsoft.adalcache");
#elif WINDOWS || MACCATALYST
        // Desktop: the in-app sign-in page stops when Microsoft redirects here
        builder = builder.WithRedirectUri("http://localhost");
#endif
        _pca = builder.Build();
    }

    /// <summary>
    /// Uses the saved session if there is one; otherwise opens the Microsoft
    /// sign-in screen. Returns null if the user closed the screen.
    /// </summary>
    public async Task<UserInfo?> SignInAsync()
    {
        AuthenticationResult? result = null;

        var account = (await _pca.GetAccountsAsync()).FirstOrDefault();
        if (account is not null)
        {
            try
            {
                result = await _pca.AcquireTokenSilent(Scopes, account).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // saved session expired: fall through to the sign-in screen
            }
        }

        if (result is null)
        {
            try
            {
                var request = _pca.AcquireTokenInteractive(Scopes)
                    .WithPrompt(Prompt.SelectAccount);
                // Show Microsoft's sign-in page inside the app, not in the browser
#if ANDROID
                request = request
                    .WithUseEmbeddedWebView(true)
                    .WithParentActivityOrWindow(
                        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity);
#elif IOS
                request = request.WithUseEmbeddedWebView(true);
#elif WINDOWS || MACCATALYST
                request = request.WithCustomWebUi(new InAppSignInWebUi());
#endif
                result = await request.ExecuteAsync();
            }
            catch (MsalClientException ex)
                when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
            {
                return null;
            }
        }

        var email = result.Account.Username;
        var name = result.ClaimsPrincipal?.FindFirst("name")?.Value ?? email;
        return new UserInfo(name, email);
    }

    public async Task SignOutAsync()
    {
        foreach (var account in await _pca.GetAccountsAsync())
            await _pca.RemoveAsync(account);
    }
}
