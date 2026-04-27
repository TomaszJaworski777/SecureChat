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

    private bool _isProcessing;
    private Brush _loginBrush;
    private Brush _registerBrush;

    private Brush _textBoxBrush;

    public LoginView()
    {
        InitializeComponent();

        MainWindow.Instance.Title.Title = "SecureChat - Login";

        _loginBrush = LoginButton.Foreground;
        _registerBrush = RegisterButton.Foreground;

        _textBoxBrush = UsernameBox.BorderBrush;

        ClearError(UsernameBox, UsernameError);
        ClearError(PasswordBox, PasswordError);
    }

    private void OnLoginClick(object sender, System.Windows.RoutedEventArgs e)
    {
        IsProcessing = true;

        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            ThrowError(PasswordBox, PasswordError, "Password can't be empty");
            IsProcessing = false;
            return;
        }

        //MainWindow.Instance.Navigate(new ChatView());
    }

    private void OnRegisterClick(object sender, System.Windows.RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(new RegisterView());
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

        if(string.IsNullOrWhiteSpace(PasswordBox.Password)) {
            ThrowError(PasswordBox, PasswordError, "Password can't be empty");
        }
    }
}
