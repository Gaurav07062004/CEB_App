using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace MyEmploylog;

// Receives the redirect  msal<client-id>://auth  after Microsoft sign-in
[Activity(Exported = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
    DataHost = "auth",
    DataScheme = "msal" + AppConfig.ClientId)]
public class MsalActivity : BrowserTabActivity
{
}
