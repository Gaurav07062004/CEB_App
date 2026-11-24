using Microsoft.Identity.Client;
using MyEmploylog.Services;

namespace MyEmploylog.Pages;

public partial class WelcomePage : ContentPage
{
    readonly AuthService _auth;
    bool _busy;

    public WelcomePage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
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

    async void OnSignOutClicked(object? sender, EventArgs e)
    {
        await _auth.SignOutAsync();
        ErrorLabel.IsVisible = false;
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
