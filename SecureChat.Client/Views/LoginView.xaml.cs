using System.Windows.Controls;

namespace SecureChat.Client.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();

        MainWindow.Instance.Title.Title = "SecureChat - Login";
    }

    private void OnLoginClick(object sender, System.Windows.RoutedEventArgs e)
    {
        // TODO: handle login
    }

    private void OnRegisterClick(object sender, System.Windows.RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(new RegisterView());
    }
}
