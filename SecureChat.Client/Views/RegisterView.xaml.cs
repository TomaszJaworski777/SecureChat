using SecureChat.Client.API;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace SecureChat.Client.Views;

public partial class RegisterView : UserControl
{
    private bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            _isProcessing = value;

            UsernameBox.IsEnabled = !_isProcessing;
            PasswordBox.IsEnabled = !_isProcessing;
            RepeatPasswordBox.IsEnabled = !_isProcessing;

            RegisterButton.IsEnabled = !_isProcessing;
            RegisterButton.Foreground = !_isProcessing ?
                _registerBrush :
                new SolidColorBrush(Color.FromRgb(96, 96, 96));

            SigninButton.IsEnabled = !_isProcessing;
            SigninButton.Foreground = !_isProcessing ?
                _signinBrush :
                new SolidColorBrush(Color.FromRgb(96, 96, 96));

            RegisterButton.Content = _isProcessing ? "PROCESSING..." : "REGISTER";
        }
    }

    private MainWindow _mainWindow;
    private Auth _auth;

    private bool _isProcessing = false;
    private Brush _registerBrush;
    private Brush _signinBrush;

    private Brush _textBoxBrush;

    public RegisterView(MainWindow mainWindow, Auth auth)
    {
        InitializeComponent();

        _mainWindow = mainWindow;
        _auth = auth;

        _mainWindow.TitleBar.Title = "SecureChat - Register";

        _registerBrush = RegisterButton.Foreground;
        _signinBrush = SigninButton.Foreground;

        _textBoxBrush = UsernameBox.BorderBrush;

        ClearError(UsernameBox, UsernameError);
        ClearError(PasswordBox, null);
        ClearError(RepeatPasswordBox, PasswordError);
    }

    private void Register_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _ = ProcessRegistration();
    }

    private void SignIn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _mainWindow.Navigate(new LoginView(_mainWindow, _auth));
    }

    private async Task ProcessRegistration() {
        IsProcessing = true;

        if (PasswordBox.Password != RepeatPasswordBox.Password)
        {
            ThrowError(PasswordBox, null, "");
            ThrowError(RepeatPasswordBox, PasswordError, "The passwords are not the same");
            IsProcessing = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(UsernameBox.Text))
        {
            ThrowError(UsernameBox, UsernameError, "Username can't be empty");
            IsProcessing = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordBox.Password)) {
            ThrowError(PasswordBox, null, "");
            ThrowError(RepeatPasswordBox, PasswordError, "Password can't be empty");
            IsProcessing = false;
            return;
        }

        switch (await _auth.RegisterAsync(UsernameBox.Text, PasswordBox.Password))
        {
            case System.Net.HttpStatusCode.OK:
                _mainWindow.Navigate(new ChatView(_mainWindow));
                break;
            case System.Net.HttpStatusCode.Conflict:
                ThrowError(UsernameBox, UsernameError, "User already exists");
                IsProcessing = false;
                break;
            default:
                IsProcessing = false;
                break;
        }
    }

    private void ThrowError(Wpf.Ui.Controls.TextBox textBox, System.Windows.Controls.TextBlock? errorBlock, string message) {
        textBox.BorderBrush = Brushes.Red;

        if (errorBlock != null)
            errorBlock.Text = message;
    }

    private void ClearError(Wpf.Ui.Controls.TextBox textBox, System.Windows.Controls.TextBlock? errorBlock)
    {
        textBox.BorderBrush = _textBoxBrush;
        
        if(errorBlock != null)
            errorBlock.Text = "";
    }

    private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ClearError(UsernameBox, UsernameError);
    }

    private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ClearError(PasswordBox, null);
        ClearError(RepeatPasswordBox, PasswordError);
    }

    private void RepeatPasswordBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ClearError(PasswordBox, null);
        ClearError(RepeatPasswordBox, PasswordError);

        if (PasswordBox.Password != RepeatPasswordBox.Password) {
            ThrowError(PasswordBox, null, "");
            ThrowError(RepeatPasswordBox, PasswordError, "The passwords are not the same");
        }
    }

    private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
            Register_Click(sender, e);
    }
}
