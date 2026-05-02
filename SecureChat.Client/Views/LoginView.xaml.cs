using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace SecureChat.Client.Views;

public partial class LoginView : UserControl
{
    private bool IsProcessing {
        get => _isProcessing;
        set {
            _isProcessing = value;

            UsernameBox.IsEnabled = !_isProcessing;
            PasswordBox.IsEnabled = !_isProcessing;

            LoginButton.IsEnabled = !_isProcessing;
            LoginButton.Foreground = !_isProcessing ?
                _loginBrush :
                new SolidColorBrush(Color.FromRgb(96, 96, 96));

            RegisterButton.IsEnabled = !_isProcessing;
            RegisterButton.Foreground = !_isProcessing ?
                _registerBrush :
                new SolidColorBrush(Color.FromRgb(96, 96, 96));

            LoginButton.Content = _isProcessing ? "PROCESSING..." : "LOG IN";
        }
    }

    private MainWindow _mainWindow;

    private bool _isProcessing;
    private Brush _loginBrush;
    private Brush _registerBrush;
    private Brush _textBoxBrush;

    public LoginView(MainWindow mainWindow)
    {
        InitializeComponent();

        _mainWindow = mainWindow;
        _mainWindow.TitleBar.Title = "SecureChat - Login";

        _ = _mainWindow.Client.ResetAsync();

        _loginBrush = LoginButton.Foreground;
        _registerBrush = RegisterButton.Foreground;

        _textBoxBrush = UsernameBox.BorderBrush;

        ClearError(UsernameBox, UsernameError);
        ClearError(PasswordBox, PasswordError);
    }

    private void Login_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _ = ProcessLogin();
    }

    private async Task ProcessLogin() {
        IsProcessing = true;

        if (string.IsNullOrWhiteSpace(UsernameBox.Text))
        {
            ThrowError(UsernameBox, UsernameError, "Username can't be empty");
            IsProcessing = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            ThrowError(PasswordBox, PasswordError, "Password can't be empty");
            IsProcessing = false;
            return;
        }

        switch (await _mainWindow.Client.LoginAsync(UsernameBox.Text, PasswordBox.Password))
        {
            case System.Net.HttpStatusCode.OK:
                var contacts = await _mainWindow.Client.GetContactsAsync();
                _mainWindow.Navigate(new ChatView(_mainWindow, contacts));
                break;
            case System.Net.HttpStatusCode.NotFound:
                ThrowError(UsernameBox, UsernameError, "User doesn't exist");
                IsProcessing = false;
                break;
            case System.Net.HttpStatusCode.Conflict:
                ThrowError(UsernameBox, UsernameError, "User is already logged in");
                IsProcessing = false;
                break;
            case System.Net.HttpStatusCode.Unauthorized:
                ThrowError(PasswordBox, PasswordError, "Incorrect password");
                IsProcessing = false;
                break;
            default:
                IsProcessing = false;
                break;
        }
    }

    private void Register_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _mainWindow.Navigate(new RegisterView(_mainWindow));
    }

    private void ThrowError(Wpf.Ui.Controls.TextBox textBox, System.Windows.Controls.TextBlock? errorBlock, string message)
    {
        textBox.BorderBrush = Brushes.Red;

        if (errorBlock != null)
            errorBlock.Text = message;
    }

    private void ClearError(Wpf.Ui.Controls.TextBox textBox, System.Windows.Controls.TextBlock? errorBlock)
    {
        textBox.BorderBrush = _textBoxBrush;

        if (errorBlock != null)
            errorBlock.Text = "";
    }

    private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ClearError(UsernameBox, UsernameError);
    }

    private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ClearError(PasswordBox, PasswordError);
    }

    private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
            Login_Click(sender, e);
    }
}
