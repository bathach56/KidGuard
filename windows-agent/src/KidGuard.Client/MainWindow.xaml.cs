using System.Net.Http;
using System.Windows;
using KidGuard.Client.Api;
using KidGuard.Client.Configuration;
using KidGuard.Client.Services;

namespace KidGuard.Client;

public partial class MainWindow : Window
{
    private const string DefaultApiBaseUrl = "http://127.0.0.1:5133";

    private readonly AuthApiClient authApiClient = new();
    private readonly DeviceApiClient deviceApiClient = new();
    private readonly PairCodeApiClient pairCodeApiClient = new();
    private readonly PairingApiClient pairingApiClient = new();
    private readonly ClientDeviceCredentialStore credentialStore = new();
    private readonly AgentServiceManager agentServiceManager = new();
    private AuthSession? authSession;
    private PairCodeSession? pairCodeSession;
    private PairingRequestSession? pairingRequestSession;
    private PendingPairingRequest? pendingPairingRequest;

    public MainWindow()
    {
        InitializeComponent();
        var configuredApiBaseUrl = ClientConfiguration.GetConfiguredApiBaseUrl();
        var apiBaseUrl = string.IsNullOrWhiteSpace(configuredApiBaseUrl)
            ? DefaultApiBaseUrl
            : configuredApiBaseUrl;

        ApiBaseUrlTextBox.Text = apiBaseUrl;
        ChildApiBaseUrlTextBox.Text = apiBaseUrl;
        ChildSetupTokenPasswordBox.Password = ClientConfiguration.GetConfiguredSetupToken();
        ChildDeviceNameTextBox.Text = Environment.MachineName;
    }

    private void ParentModeButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(ParentPanel);
        ShowParentDashboard(authSession is not null);
        ShowLoginForm();
    }

    private void ChildModeButton_Click(object sender, RoutedEventArgs e)
    {
        ResetChildRequestView();
        ShowPanel(ChildPanel);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(RoleSelectionPanel);
    }

    private void LoginTabButton_Click(object sender, RoutedEventArgs e)
    {
        ShowLoginForm();
    }

    private void RegisterTabButton_Click(object sender, RoutedEventArgs e)
    {
        ShowRegisterForm();
    }

    private async void CreateCodeButton_Click(object sender, RoutedEventArgs e)
    {
        var apiBaseUrl = ChildApiBaseUrlTextBox.Text.Trim();
        var deviceName = ChildDeviceNameTextBox.Text.Trim();

        if (!TryValidateChildCodeInput(apiBaseUrl, deviceName, out var baseUri, out var validationMessage))
        {
            SetChildStatus(validationMessage, isError: true);
            return;
        }

        SetCreateCodeLoadingState(isLoading: true);
        SetChildStatus("Creating code...", isError: false);

        try
        {
            pairCodeSession = await pairCodeApiClient.CreatePairCodeAsync(
                baseUri,
                deviceName,
                Environment.MachineName,
                CancellationToken.None);

            ConnectionCodeTextBlock.Text = pairCodeSession.ConnectionCode;
            PendingRequestTextBlock.Text = "No parent request yet.";
            pendingPairingRequest = null;
            PendingRequestPanel.Visibility = Visibility.Collapsed;
            ChildDecisionPanel.Visibility = Visibility.Collapsed;
            CheckPendingRequestButton.Visibility = Visibility.Visible;
            SetChildDecisionButtonsEnabled(isEnabled: false);
            SetChildStatus($"Code expires in {pairCodeSession.ExpiresInSeconds} seconds.", isError: false);
        }
        catch (HttpRequestException exception)
        {
            SetChildStatus($"Cannot connect to Backend: {exception.Message}", isError: true);
        }
        catch (InvalidOperationException exception)
        {
            SetChildStatus(exception.Message, isError: true);
        }
        catch (TaskCanceledException)
        {
            SetChildStatus("Create code request timed out or was canceled.", isError: true);
        }
        finally
        {
            SetCreateCodeLoadingState(isLoading: false);
        }
    }

    private async void SendPairRequestButton_Click(object sender, RoutedEventArgs e)
    {
        var apiBaseUrl = ApiBaseUrlTextBox.Text.Trim();
        var pairCode = ChildCodeTextBox.Text.Trim().ToUpperInvariant();

        if (!TryValidatePairInput(apiBaseUrl, pairCode, out var baseUri, out var validationMessage))
        {
            SetPairingStatus(validationMessage, isError: true);
            return;
        }

        SetPairRequestLoadingState(isLoading: true);
        UpdatePairingState("Waiting", "Sending pairing request...", PairingStateKind.Waiting);

        try
        {
            pairingRequestSession = await pairingApiClient.CreateParentPairingRequestAsync(
                baseUri,
                authSession!.AccessToken,
                pairCode,
                CancellationToken.None);

            ShowPairingRequest(pairingRequestSession);
            UpdatePairingState(
                pairingRequestSession.Status,
                "Pairing request sent. Ask the child to approve it, then refresh status.",
                PairingStateKind.Waiting);
        }
        catch (HttpRequestException exception)
        {
            UpdatePairingState("Failed", $"Cannot connect to Backend: {exception.Message}", PairingStateKind.Error);
        }
        catch (InvalidOperationException exception)
        {
            UpdatePairingState("Failed", exception.Message, PairingStateKind.Error);
        }
        catch (TaskCanceledException)
        {
            UpdatePairingState("Failed", "Pairing request timed out or was canceled.", PairingStateKind.Error);
        }
        finally
        {
            SetPairRequestLoadingState(isLoading: false);
        }
    }

    private async void RefreshPairingStatusButton_Click(object sender, RoutedEventArgs e)
    {
        var apiBaseUrl = ApiBaseUrlTextBox.Text.Trim();

        if (pairingRequestSession is null)
        {
            UpdatePairingState("Not started", "Send a pairing request before refreshing status.", PairingStateKind.Error);
            return;
        }

        if (!TryValidateDeviceListInput(apiBaseUrl, out var baseUri, out var validationMessage))
        {
            UpdatePairingState(pairingRequestSession.Status, validationMessage, PairingStateKind.Error);
            return;
        }

        SetPairRequestLoadingState(isLoading: true);

        try
        {
            pairingRequestSession = await pairingApiClient.GetPairingStatusAsync(
                baseUri,
                authSession!.AccessToken,
                pairingRequestSession.PairingRequestId,
                CancellationToken.None);

            ShowPairingRequest(pairingRequestSession);
            UpdatePairingState(
                pairingRequestSession.Status,
                $"Pairing status is {pairingRequestSession.Status}.",
                GetPairingStateKind(pairingRequestSession.Status));

            if (string.Equals(pairingRequestSession.Status, "approved", StringComparison.OrdinalIgnoreCase))
            {
                await LoadDevicesAsync(baseUri, authSession.AccessToken);
            }
        }
        catch (HttpRequestException exception)
        {
            UpdatePairingState(pairingRequestSession.Status, $"Cannot connect to Backend: {exception.Message}", PairingStateKind.Error);
        }
        catch (InvalidOperationException exception)
        {
            UpdatePairingState(pairingRequestSession.Status, exception.Message, PairingStateKind.Error);
        }
        catch (TaskCanceledException)
        {
            UpdatePairingState(pairingRequestSession.Status, "Pairing status request timed out or was canceled.", PairingStateKind.Error);
        }
        finally
        {
            SetPairRequestLoadingState(isLoading: false);
        }
    }

    private async void ParentRegisterButton_Click(object sender, RoutedEventArgs e)
    {
        var apiBaseUrl = ApiBaseUrlTextBox.Text.Trim();
        var email = ParentRegisterEmailTextBox.Text.Trim();
        var password = ParentRegisterPasswordBox.Password;
        var confirmPassword = ParentRegisterConfirmPasswordBox.Password;
        var fullName = ParentFullNameTextBox.Text.Trim();
        var phoneNumber = ParentPhoneNumberTextBox.Text.Trim();

        if (!TryValidateRegisterInput(apiBaseUrl, email, password, confirmPassword, fullName, out var baseUri, out var validationMessage))
        {
            SetParentStatus(validationMessage, isError: true);
            return;
        }

        SetLoginLoadingState(isLoading: true);
        SetParentStatus("Registering...", isError: false);

        try
        {
            await authApiClient.RegisterAsync(baseUri, email, password, fullName, phoneNumber, CancellationToken.None);
            SetParentStatus("Register successful. You can login now.", isError: false);
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
            SetParentStatus("Register request timed out or was canceled.", isError: true);
        }
        finally
        {
            SetLoginLoadingState(isLoading: false);
        }
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
            ShowParentDashboard(isLoggedIn: true);
            await LoadDevicesAsync(baseUri, authSession.AccessToken);
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

    private async void RefreshDevicesButton_Click(object sender, RoutedEventArgs e)
    {
        var apiBaseUrl = ApiBaseUrlTextBox.Text.Trim();
        if (!TryValidateDeviceListInput(apiBaseUrl, out var baseUri, out var validationMessage))
        {
            SetDeviceListStatus(validationMessage, isError: true);
            return;
        }

        await LoadDevicesAsync(baseUri, authSession!.AccessToken);
    }

    private void DeviceListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (DeviceListBox.SelectedItem is not DeviceSummary device)
        {
            SetDeviceActionsEnabled(isEnabled: false);
            ModeStatusTextBlock.Text = "Select an approved device to change mode.";
            LogsStatusTextBlock.Text = "Select a device to load logs.";
            return;
        }

        SelectMode(device.Mode);
        SetDeviceActionsEnabled(isEnabled: true);
        ModeStatusTextBlock.Text = $"Selected {device.DeviceName}.";
        LogsStatusTextBlock.Text = "Refresh logs to view recent activity.";
    }

    private async void UpdateModeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedDevice(out var device, out var validationMessage))
        {
            SetModeStatus(validationMessage, isError: true);
            return;
        }

        if (!TryValidateDeviceListInput(ApiBaseUrlTextBox.Text.Trim(), out var baseUri, out validationMessage))
        {
            SetModeStatus(validationMessage, isError: true);
            return;
        }

        var selectedMode = GetSelectedMode();
        if (string.IsNullOrWhiteSpace(selectedMode))
        {
            SetModeStatus("Select a mode before updating.", isError: true);
            return;
        }

        SetDeviceActionLoadingState(isLoading: true);
        SetModeStatus($"Updating {device.DeviceName} to {selectedMode}...", isError: false);

        try
        {
            var updatedMode = await deviceApiClient.UpdateDeviceModeAsync(
                baseUri,
                authSession!.AccessToken,
                device.DeviceId,
                selectedMode,
                CancellationToken.None);

            SetModeStatus($"Mode updated to {updatedMode}.", isError: false);
            await LoadDevicesAsync(baseUri, authSession.AccessToken);
        }
        catch (HttpRequestException exception)
        {
            SetModeStatus($"Cannot connect to Backend: {exception.Message}", isError: true);
        }
        catch (InvalidOperationException exception)
        {
            SetModeStatus(exception.Message, isError: true);
        }
        catch (TaskCanceledException)
        {
            SetModeStatus("Mode update timed out or was canceled.", isError: true);
        }
        finally
        {
            SetDeviceActionLoadingState(isLoading: false);
        }
    }

    private async void RefreshLogsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedDevice(out var device, out var validationMessage))
        {
            SetLogsStatus(validationMessage, isError: true);
            return;
        }

        if (!TryValidateDeviceListInput(ApiBaseUrlTextBox.Text.Trim(), out var baseUri, out validationMessage))
        {
            SetLogsStatus(validationMessage, isError: true);
            return;
        }

        SetDeviceActionLoadingState(isLoading: true);
        SetLogsStatus($"Loading logs for {device.DeviceName}...", isError: false);

        try
        {
            var logs = await deviceApiClient.GetDeviceLogsAsync(
                baseUri,
                authSession!.AccessToken,
                device.DeviceId,
                CancellationToken.None);

            DeviceLogsListBox.ItemsSource = logs;
            SetLogsStatus(
                logs.Count == 0
                    ? "No logs found for this device."
                    : $"{logs.Count} recent log(s) loaded.",
                isError: false);
        }
        catch (HttpRequestException exception)
        {
            SetLogsStatus($"Cannot connect to Backend: {exception.Message}", isError: true);
        }
        catch (InvalidOperationException exception)
        {
            SetLogsStatus(exception.Message, isError: true);
        }
        catch (TaskCanceledException)
        {
            SetLogsStatus("Log request timed out or was canceled.", isError: true);
        }
        finally
        {
            SetDeviceActionLoadingState(isLoading: false);
        }
    }

    private void CopyDeviceTokenButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DeviceTokenTextBox.Text))
        {
            UpdatePairingState("Paired", "No Device Token is available to copy.", PairingStateKind.Error);
            return;
        }

        Clipboard.SetText(DeviceTokenTextBox.Text);
        UpdatePairingState("Paired", "Device Token copied to clipboard.", PairingStateKind.Success);
    }

    private async void CheckPendingRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryValidatePendingRequestInput(out var baseUri, out var connectionCode, out var validationMessage))
        {
            SetChildStatus(validationMessage, isError: true);
            return;
        }

        SetChildPendingLoadingState(isLoading: true);
        SetChildStatus("Checking pending request...", isError: false);

        try
        {
            pendingPairingRequest = await pairingApiClient.GetChildPendingRequestAsync(
                baseUri,
                connectionCode,
                CancellationToken.None);

            if (pendingPairingRequest is null)
            {
                PendingRequestTextBlock.Text = "No parent request yet.";
                PendingRequestPanel.Visibility = Visibility.Collapsed;
                ChildDecisionPanel.Visibility = Visibility.Collapsed;
                SetChildDecisionButtonsEnabled(isEnabled: false);
                SetChildStatus("No pending request found.", isError: false);
                return;
            }

            PendingRequestTextBlock.Text = pendingPairingRequest.DisplayText;
            PendingRequestPanel.Visibility = Visibility.Visible;
            ChildDecisionPanel.Visibility = Visibility.Visible;
            SetChildDecisionButtonsEnabled(isEnabled: true);
            SetChildStatus("Parent request found. Grant permission to pair, or deny to reject pairing.", isError: false);
        }
        catch (HttpRequestException exception)
        {
            SetChildStatus($"Cannot connect to Backend: {exception.Message}", isError: true);
        }
        catch (InvalidOperationException exception)
        {
            SetChildStatus(exception.Message, isError: true);
        }
        catch (TaskCanceledException)
        {
            SetChildStatus("Pending request check timed out or was canceled.", isError: true);
        }
        finally
        {
            SetChildPendingLoadingState(isLoading: false);
        }
    }

    private async void ApprovePairingButton_Click(object sender, RoutedEventArgs e)
    {
        await SubmitChildPairingDecisionAsync(approve: true);
    }

    private async void RejectPairingButton_Click(object sender, RoutedEventArgs e)
    {
        await SubmitChildPairingDecisionAsync(approve: false);
    }

    private void ShowPanel(UIElement activePanel)
    {
        RoleSelectionPanel.Visibility = Visibility.Collapsed;
        ParentPanel.Visibility = Visibility.Collapsed;
        ChildPanel.Visibility = Visibility.Collapsed;

        activePanel.Visibility = Visibility.Visible;
    }

    private void ShowParentDashboard(bool isLoggedIn)
    {
        ParentAuthPanel.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
        ParentDashboardPanel.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ShowLoginForm()
    {
        LoginFormPanel.Visibility = Visibility.Visible;
        RegisterFormPanel.Visibility = Visibility.Collapsed;
        LoginTabButton.Style = (Style)FindResource("PrimaryButtonStyle");
        RegisterTabButton.Style = (Style)FindResource("SecondaryButtonStyle");
    }

    private void ShowRegisterForm()
    {
        LoginFormPanel.Visibility = Visibility.Collapsed;
        RegisterFormPanel.Visibility = Visibility.Visible;
        LoginTabButton.Style = (Style)FindResource("SecondaryButtonStyle");
        RegisterTabButton.Style = (Style)FindResource("PrimaryButtonStyle");
    }

    private void ResetChildRequestView()
    {
        if (pairCodeSession is null)
        {
            CheckPendingRequestButton.Visibility = Visibility.Collapsed;
        }

        PendingRequestPanel.Visibility = Visibility.Collapsed;
        ChildDecisionPanel.Visibility = Visibility.Collapsed;
        PendingRequestTextBlock.Text = "No parent request yet.";
        pendingPairingRequest = null;
        SetChildDecisionButtonsEnabled(isEnabled: false);
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

        if (!TryValidateApiBaseUrl(apiBaseUrl, out baseUri, out message))
        {
            return false;
        }

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

    private bool TryValidateDeviceListInput(
        string apiBaseUrl,
        out Uri baseUri,
        out string message)
    {
        baseUri = default!;
        message = string.Empty;

        if (authSession is null)
        {
            message = "Parent login is required before loading devices.";
            return false;
        }

        return TryValidateApiBaseUrl(apiBaseUrl, out baseUri, out message);
    }

    private static bool TryValidateChildCodeInput(
        string apiBaseUrl,
        string deviceName,
        out Uri baseUri,
        out string message)
    {
        baseUri = default!;
        message = string.Empty;

        if (!TryValidateApiBaseUrl(apiBaseUrl, out baseUri, out message))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(deviceName))
        {
            message = "Device name is required.";
            return false;
        }

        return true;
    }

    private static bool TryValidateRegisterInput(
        string apiBaseUrl,
        string email,
        string password,
        string confirmPassword,
        string fullName,
        out Uri baseUri,
        out string message)
    {
        if (!TryValidateLoginInput(apiBaseUrl, email, password, out baseUri, out message))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            message = "Username is required.";
            return false;
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            message = "Confirm password must match password.";
            return false;
        }

        return true;
    }

    private bool TryValidatePendingRequestInput(
        out Uri baseUri,
        out string connectionCode,
        out string message)
    {
        baseUri = default!;
        connectionCode = pairCodeSession?.ConnectionCode ?? string.Empty;
        message = string.Empty;

        if (!TryValidateApiBaseUrl(ChildApiBaseUrlTextBox.Text.Trim(), out baseUri, out message))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(connectionCode))
        {
            message = "Create a child connection code before checking pending requests.";
            return false;
        }

        return true;
    }

    private bool TryValidatePairInput(
        string apiBaseUrl,
        string pairCode,
        out Uri baseUri,
        out string message)
    {
        baseUri = default!;
        message = string.Empty;

        if (authSession is null)
        {
            message = "Parent login is required before pairing.";
            return false;
        }

        if (!TryValidateApiBaseUrl(apiBaseUrl, out baseUri, out message))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(pairCode))
        {
            message = "Child connection code is required.";
            return false;
        }

        return true;
    }

    private static bool TryValidateApiBaseUrl(string apiBaseUrl, out Uri baseUri, out string message)
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
        return true;
    }

    private void SetLoginLoadingState(bool isLoading)
    {
        ParentLoginButton.IsEnabled = !isLoading;
        ParentRegisterButton.IsEnabled = !isLoading;
        LoginTabButton.IsEnabled = !isLoading;
        RegisterTabButton.IsEnabled = !isLoading;
        ParentLoginButton.Content = isLoading ? "Logging in" : "Login";
        ParentRegisterButton.Content = isLoading ? "Creating account" : "Create account";
        ParentEmailTextBox.IsEnabled = !isLoading;
        ParentPasswordBox.IsEnabled = !isLoading;
        ParentRegisterEmailTextBox.IsEnabled = !isLoading;
        ParentRegisterPasswordBox.IsEnabled = !isLoading;
        ParentRegisterConfirmPasswordBox.IsEnabled = !isLoading;
        ParentFullNameTextBox.IsEnabled = !isLoading;
        ParentPhoneNumberTextBox.IsEnabled = !isLoading;
        ApiBaseUrlTextBox.IsEnabled = !isLoading;
    }

    private void SetPairRequestLoadingState(bool isLoading)
    {
        SendPairRequestButton.IsEnabled = !isLoading;
        RefreshPairingStatusButton.IsEnabled = !isLoading;
        SendPairRequestButton.Content = isLoading ? "Sending" : "Send Request";
        ChildCodeTextBox.IsEnabled = !isLoading;
    }

    private async Task LoadDevicesAsync(Uri apiBaseUrl, string accessToken)
    {
        SetDeviceListLoadingState(isLoading: true);
        SetDeviceListStatus("Loading devices...", isError: false);

        try
        {
            var devices = await deviceApiClient.GetDevicesAsync(apiBaseUrl, accessToken, CancellationToken.None);
            DeviceListBox.ItemsSource = devices;
            SetDeviceActionsEnabled(devices.Count > 0 && DeviceListBox.SelectedItem is DeviceSummary);
            SetDeviceListStatus(
                devices.Count == 0
                    ? "No approved devices yet."
                    : $"{devices.Count} approved device(s) loaded.",
                isError: false);
        }
        catch (HttpRequestException exception)
        {
            SetDeviceListStatus($"Cannot connect to Backend: {exception.Message}", isError: true);
        }
        catch (InvalidOperationException exception)
        {
            SetDeviceListStatus(exception.Message, isError: true);
        }
        catch (TaskCanceledException)
        {
            SetDeviceListStatus("Device list request timed out or was canceled.", isError: true);
        }
        finally
        {
            SetDeviceListLoadingState(isLoading: false);
        }
    }

    private void SetDeviceListLoadingState(bool isLoading)
    {
        RefreshDevicesButton.IsEnabled = !isLoading;
        RefreshDevicesButton.Content = isLoading ? "Loading" : "Refresh";
    }

    private bool TryGetSelectedDevice(out DeviceSummary device, out string message)
    {
        if (DeviceListBox.SelectedItem is DeviceSummary selectedDevice)
        {
            device = selectedDevice;
            message = string.Empty;
            return true;
        }

        device = default!;
        message = "Select an approved device first.";
        return false;
    }

    private string? GetSelectedMode()
    {
        return DeviceModeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedMode
            ? selectedMode.Content?.ToString()
            : null;
    }

    private void SelectMode(string mode)
    {
        foreach (var item in DeviceModeComboBox.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem comboBoxItem
                && string.Equals(comboBoxItem.Content?.ToString(), mode, StringComparison.OrdinalIgnoreCase))
            {
                DeviceModeComboBox.SelectedItem = comboBoxItem;
                return;
            }
        }
    }

    private void SetDeviceActionsEnabled(bool isEnabled)
    {
        DeviceModeComboBox.IsEnabled = isEnabled;
        UpdateModeButton.IsEnabled = isEnabled;
        RefreshLogsButton.IsEnabled = isEnabled;
    }

    private void SetDeviceActionLoadingState(bool isLoading)
    {
        var hasSelectedDevice = DeviceListBox.SelectedItem is DeviceSummary;
        DeviceModeComboBox.IsEnabled = !isLoading && hasSelectedDevice;
        UpdateModeButton.IsEnabled = !isLoading && hasSelectedDevice;
        RefreshLogsButton.IsEnabled = !isLoading && hasSelectedDevice;
        UpdateModeButton.Content = isLoading ? "Updating" : "Update Mode";
        RefreshLogsButton.Content = isLoading ? "Loading" : "Refresh Logs";
    }

    private void SetModeStatus(string message, bool isError)
    {
        ModeStatusTextBlock.Text = message;
        ModeStatusTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.ForestGreen;
    }

    private void SetLogsStatus(string message, bool isError)
    {
        LogsStatusTextBlock.Text = message;
        LogsStatusTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.ForestGreen;
    }

    private void SetParentStatus(string message, bool isError)
    {
        ParentLoginStatusTextBlock.Text = message;
        ParentLoginStatusTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.ForestGreen;
    }

    private void SetPairingStatus(string message, bool isError)
    {
        PairingStatusTextBlock.Text = message;
        PairingStatusTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.ForestGreen;
    }

    private void UpdatePairingState(string state, string message, PairingStateKind stateKind)
    {
        PairingStateTextBlock.Text = state;
        PairingStatusTextBlock.Text = message;

        switch (stateKind)
        {
            case PairingStateKind.Waiting:
                PairingStateBadge.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
                PairingStateBadge.BorderBrush = System.Windows.Media.Brushes.Goldenrod;
                PairingStateTextBlock.Foreground = System.Windows.Media.Brushes.DarkGoldenrod;
                PairingStatusTextBlock.Foreground = System.Windows.Media.Brushes.DarkGoldenrod;
                break;
            case PairingStateKind.Success:
                PairingStateBadge.Background = System.Windows.Media.Brushes.Honeydew;
                PairingStateBadge.BorderBrush = System.Windows.Media.Brushes.DarkSeaGreen;
                PairingStateTextBlock.Foreground = System.Windows.Media.Brushes.ForestGreen;
                PairingStatusTextBlock.Foreground = System.Windows.Media.Brushes.ForestGreen;
                break;
            case PairingStateKind.Error:
                PairingStateBadge.Background = System.Windows.Media.Brushes.MistyRose;
                PairingStateBadge.BorderBrush = System.Windows.Media.Brushes.IndianRed;
                PairingStateTextBlock.Foreground = System.Windows.Media.Brushes.Firebrick;
                PairingStatusTextBlock.Foreground = System.Windows.Media.Brushes.Firebrick;
                break;
            default:
                PairingStateBadge.Background = System.Windows.Media.Brushes.AliceBlue;
                PairingStateBadge.BorderBrush = System.Windows.Media.Brushes.LightSteelBlue;
                PairingStateTextBlock.Foreground = System.Windows.Media.Brushes.SteelBlue;
                PairingStatusTextBlock.Foreground = System.Windows.Media.Brushes.DimGray;
                break;
        }
    }

    private void ShowPairedDevice(PairedDevice device)
    {
        PairedDeviceNameTextBlock.Text = device.DeviceName;
        PairedDeviceIdTextBlock.Text = $"Device ID: {device.DeviceId}";
        PairedDeviceModeTextBlock.Text = $"Mode: {device.Mode}";
        PairedDevicePanel.Visibility = Visibility.Visible;
        DeviceTokenTextBox.Text = device.DeviceToken;
        DeviceTokenTextBox.Visibility = Visibility.Visible;
        CopyDeviceTokenButton.Visibility = Visibility.Visible;
    }

    private void ShowPairingRequest(PairingRequestSession request)
    {
        PairedDeviceNameTextBlock.Text = request.DeviceName;
        PairedDeviceIdTextBlock.Text = $"Device ID: {request.DeviceId}";
        PairedDeviceModeTextBlock.Text = $"Computer: {request.ComputerName}";
        PairedDevicePanel.Visibility = Visibility.Visible;
        DeviceTokenTextBox.Visibility = Visibility.Collapsed;
        CopyDeviceTokenButton.Visibility = Visibility.Collapsed;
    }

    private static PairingStateKind GetPairingStateKind(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "pending" => PairingStateKind.Waiting,
            "approved" => PairingStateKind.Success,
            "rejected" or "expired" => PairingStateKind.Error,
            _ => PairingStateKind.Neutral
        };
    }

    private void SetDeviceListStatus(string message, bool isError)
    {
        DeviceListStatusTextBlock.Text = message;
        DeviceListStatusTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.ForestGreen;
    }

    private void SetCreateCodeLoadingState(bool isLoading)
    {
        CreateCodeButton.IsEnabled = !isLoading;
        CreateCodeButton.Content = isLoading ? "Creating" : "Create Code";
        ChildApiBaseUrlTextBox.IsEnabled = !isLoading;
        ChildSetupTokenPasswordBox.IsEnabled = !isLoading;
        ChildDeviceNameTextBox.IsEnabled = !isLoading;
    }

    private void SetChildPendingLoadingState(bool isLoading)
    {
        CheckPendingRequestButton.IsEnabled = !isLoading;
        CheckPendingRequestButton.Content = isLoading ? "Checking" : "Check";
        if (pendingPairingRequest is not null)
        {
            SetChildDecisionButtonsEnabled(!isLoading);
        }
    }

    private void SetChildDecisionButtonsEnabled(bool isEnabled)
    {
        ApprovePairingButton.IsEnabled = isEnabled;
        RejectPairingButton.IsEnabled = isEnabled;
    }

    private void SetChildStatus(string message, bool isError)
    {
        ChildCodeStatusTextBlock.Text = message;
        ChildCodeStatusTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.ForestGreen;
    }

    private async Task SubmitChildPairingDecisionAsync(bool approve)
    {
        if (!TryValidatePendingRequestInput(out var baseUri, out var connectionCode, out var validationMessage))
        {
            SetChildStatus(validationMessage, isError: true);
            return;
        }

        if (pendingPairingRequest is null)
        {
            SetChildStatus("Check and select a pending request before deciding.", isError: true);
            return;
        }

        SetChildPendingLoadingState(isLoading: true);
        SetChildStatus(approve ? "Granting permission..." : "Denying permission...", isError: false);

        try
        {
            var result = approve
                ? await pairingApiClient.ApprovePairingAsync(
                    baseUri,
                    pendingPairingRequest.PairingRequestId,
                    connectionCode,
                    CancellationToken.None)
                : await pairingApiClient.RejectPairingAsync(
                    baseUri,
                    pendingPairingRequest.PairingRequestId,
                    connectionCode,
                    CancellationToken.None);

            if (approve && result.DeviceId is Guid deviceId && !string.IsNullOrWhiteSpace(result.DeviceToken))
            {
                await credentialStore.SaveCredentialsAsync(deviceId, result.DeviceToken, CancellationToken.None);
                var serviceStatus = await agentServiceManager.GetStatusAsync(CancellationToken.None);
                PendingRequestTextBlock.Text = $"Approved {result.DeviceName}. Credentials saved for the Windows Service. {serviceStatus.Message}";
            }
            else
            {
                PendingRequestTextBlock.Text = "Permission denied. Pairing request rejected.";
            }

            pendingPairingRequest = null;
            CheckPendingRequestButton.Visibility = Visibility.Collapsed;
            ChildDecisionPanel.Visibility = Visibility.Collapsed;
            SetChildDecisionButtonsEnabled(isEnabled: false);
            SetChildStatus($"Pairing {result.Status}.", isError: false);
        }
        catch (HttpRequestException exception)
        {
            SetChildStatus($"Cannot connect to Backend: {exception.Message}", isError: true);
        }
        catch (InvalidOperationException exception)
        {
            SetChildStatus(exception.Message, isError: true);
        }
        catch (TaskCanceledException)
        {
            SetChildStatus("Pairing decision timed out or was canceled.", isError: true);
        }
        catch (UnauthorizedAccessException exception)
        {
            SetChildStatus($"Cannot save credentials. Run KidGuard as Administrator: {exception.Message}", isError: true);
        }
        finally
        {
            SetChildPendingLoadingState(isLoading: false);
        }
    }

    private enum PairingStateKind
    {
        Neutral,
        Waiting,
        Success,
        Error
    }
}
