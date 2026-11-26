using Microsoft.Identity.Client;
using MyEmploylog.Services;

namespace MyEmploylog.Pages;

public partial class WelcomePage : ContentPage
{
    readonly AuthService _auth;
    readonly CebApiService _api;
    bool _busy;

    public WelcomePage(AuthService auth, CebApiService api)
    {
        InitializeComponent();
        _auth = auth;
        _api = api;
    }

    // App opens: connect to Microsoft straight away
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SignInAsync();
    }

    async void OnSignInClicked(object? sender, EventArgs e) => await SignInAsync();

    async Task SignInAsync()
    {
        if (_busy) return;
        _busy = true;
        Show(busy: true);

        try
        {
            var user = await _auth.SignInAsync();
            if (user is null)
            {
                Show(signIn: true);              // user closed the Microsoft screen
                return;
            }

            NameLabel.Text = user.Name;
            EmailLabel.Text = user.Email;
            Show(welcome: true);

            await LoadDetailsAsync(user.Email);
        }
        catch (MsalException ex)
        {
            ErrorLabel.Text = $"Sign-in failed: {ex.Message}";
            ErrorLabel.IsVisible = true;
            Show(signIn: true);
        }
        finally
        {
            _busy = false;
        }
    }

    // After sign-in: ask the CEB web service about this user
    async Task LoadDetailsAsync(string email)
    {
        DetailsBusy.IsVisible = DetailsBusy.IsRunning = true;
        DetailsLabel.ClearValue(Label.TextColorProperty);
        DetailsLabel.Text = "";

        try
        {
            var json = await _api.GetUserJsonAsync(CebApiService.ToServiceUser(email));
            DetailsLabel.Text = CebApiService.ToDisplayText(json);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            DetailsLabel.TextColor = Colors.Red;
            DetailsLabel.Text = ex is TaskCanceledException
                ? "The CEB web service did not answer in time. Are you on the CEB network?"
                : $"Could not reach the CEB web service: {ex.Message}";
        }
        finally
        {
            DetailsBusy.IsVisible = DetailsBusy.IsRunning = false;
        }
    }

    async void OnSignOutClicked(object? sender, EventArgs e)
    {
        await _auth.SignOutAsync();
        ErrorLabel.IsVisible = false;
        DetailsLabel.Text = "";
        Show(signIn: true);
    }

    void Show(bool busy = false, bool welcome = false, bool signIn = false)
    {
        Busy.IsVisible = busy;
        Busy.IsRunning = busy;
        WelcomePanel.IsVisible = welcome;
        SignInPanel.IsVisible = signIn;
    }
}
