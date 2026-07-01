using System.Net.Http;
using System.Windows;
using KidGuard.Client.Api;
using KidGuard.Client.Configuration;

namespace KidGuard.Client;

public partial class MainWindow : Window
{
    private readonly AuthApiClient authApiClient = new();
    private AuthSession? authSession;

    public MainWindow()
    {
        InitializeComponent();
        ApiBaseUrlTextBox.Text = ClientConfiguration.GetConfiguredApiBaseUrl();
    }

    private void ParentModeButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(ParentPanel);
    }

    private void ChildModeButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(ChildPanel);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(RoleSelectionPanel);
    }

    private async void ParentLoginButton_Click(object sender, RoutedEventArgs e)
    {
        var apiBaseUrl = ApiBaseUrlTextBox.Text.Trim();
        var email = ParentEmailTextBox.Text.Trim();
        var password = ParentPasswordBox.Password;

        if (!TryValidateLoginInput(apiBaseUrl, email, password, out var baseUri, out var validationMessage))
        {
            SetParentStatus(validationMessage, isError: true);
            return;
        }

        SetLoginLoadingState(isLoading: true);
        SetParentStatus("Logging in...", isError: false);

        try
        {
            authSession = await authApiClient.LoginAsync(baseUri, email, password, CancellationToken.None);
            SetParentStatus($"Login successful. Token expires in {authSession.ExpiresIn} seconds.", isError: false);
        }
        catch (HttpRequestException exception)
        {
            SetParentStatus($"Cannot connect to Backend: {exception.Message}", isError: true);
        }
        catch (InvalidOperationException exception)
        {
            SetParentStatus(exception.Message, isError: true);
        }
        catch (TaskCanceledException)
        {
            SetParentStatus("Login request timed out or was canceled.", isError: true);
        }
        finally
        {
            SetLoginLoadingState(isLoading: false);
        }
    }

    private void ShowPanel(UIElement activePanel)
    {
        RoleSelectionPanel.Visibility = Visibility.Collapsed;
        ParentPanel.Visibility = Visibility.Collapsed;
        ChildPanel.Visibility = Visibility.Collapsed;

        activePanel.Visibility = Visibility.Visible;
    }

    private static bool TryValidateLoginInput(
        string apiBaseUrl,
        string email,
        string password,
        out Uri baseUri,
        out string message)
    {
        baseUri = default!;
        message = string.Empty;

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            message = $"API base URL is required. Set {ClientConfiguration.ApiBaseUrlEnvironmentVariable} or enter it here.";
            return false;
        }

        if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var parsedBaseUri)
            || (parsedBaseUri.Scheme != Uri.UriSchemeHttp && parsedBaseUri.Scheme != Uri.UriSchemeHttps))
        {
            message = "API base URL must be an absolute HTTP or HTTPS URL.";
            return false;
        }

        baseUri = parsedBaseUri;

        if (string.IsNullOrWhiteSpace(email))
        {
            message = "Email is required.";
            return false;
        }

        if (password.Length < 8)
        {
            message = "Password must be at least 8 characters.";
            return false;
        }

        return true;
    }

    private void SetLoginLoadingState(bool isLoading)
    {
        ParentLoginButton.IsEnabled = !isLoading;
        ParentLoginButton.Content = isLoading ? "Logging in" : "Login";
        ParentEmailTextBox.IsEnabled = !isLoading;
        ParentPasswordBox.IsEnabled = !isLoading;
        ApiBaseUrlTextBox.IsEnabled = !isLoading;
    }

    private void SetParentStatus(string message, bool isError)
    {
        ParentLoginStatusTextBlock.Text = message;
        ParentLoginStatusTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.ForestGreen;
    }
}
