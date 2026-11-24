using Foundation;
using Microsoft.Identity.Client;
using UIKit;

namespace MyEmploylog;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    // Receives the redirect  msauth.<bundle-id>://auth  after Microsoft sign-in
    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
        return base.OpenUrl(application, url, options);
    }
}
