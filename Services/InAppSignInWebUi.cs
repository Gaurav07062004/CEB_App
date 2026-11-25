#if WINDOWS || MACCATALYST
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;

namespace MyEmploylog.Services;

/// <summary>
/// Shows Microsoft's sign-in page in a WebView inside the app (Windows and Mac).
/// When Microsoft redirects to the registered redirect URI, the page closes and
/// the address (which carries the one-time code) is handed back to MSAL.
/// </summary>
public class InAppSignInWebUi : ICustomWebUi
{
    public async Task<Uri> AcquireAuthorizationCodeAsync(
        Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken)
    {
        var done = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);
        var redirect = redirectUri.GetLeftPart(UriPartial.Path).TrimEnd('/');

        var page = await MainThread.InvokeOnMainThreadAsync(() =>
            new SignInPage(authorizationUri, redirect, done));

        using var _ = cancellationToken.Register(() =>
            done.TrySetException(new MsalClientException(
                MsalError.AuthenticationCanceledError, "Sign-in was cancelled.")));

        var navigation = Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation
            ?? throw new InvalidOperationException("No window is open to show the sign-in page.");

        await MainThread.InvokeOnMainThreadAsync(() => navigation.PushModalAsync(page));

        try
        {
            return await done.Task;
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (navigation.ModalStack.Contains(page))
                    await navigation.PopModalAsync();
            });
        }
    }

    sealed class SignInPage : ContentPage
    {
        readonly TaskCompletionSource<Uri> _done;

        public SignInPage(Uri authorizationUri, string redirect, TaskCompletionSource<Uri> done)
        {
            _done = done;
            Title = "Sign in";

            var webView = new WebView { Source = authorizationUri.AbsoluteUri };
            webView.Navigating += (_, e) =>
            {
                // Microsoft is sending the user back to the app: stop and return the code
                if (e.Url.StartsWith(redirect, StringComparison.OrdinalIgnoreCase))
                {
                    e.Cancel = true;
                    _done.TrySetResult(new Uri(e.Url));
                }
            };

            var cancel = new Button { Text = "Cancel", HorizontalOptions = LayoutOptions.Start };
            cancel.Clicked += (_, _) => Cancel();

            var grid = new Grid
            {
                RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) },
                Padding = new Thickness(8),
                RowSpacing = 8,
            };
            grid.Add(cancel, 0, 0);
            grid.Add(webView, 0, 1);
            Content = grid;
        }

        // Closing the page without finishing counts as cancelling
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Cancel();
        }

        void Cancel() => _done.TrySetException(new MsalClientException(
            MsalError.AuthenticationCanceledError, "Sign-in was cancelled."));
    }
}
#endif
