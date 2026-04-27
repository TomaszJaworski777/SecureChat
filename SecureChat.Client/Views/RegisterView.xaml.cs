using System.Windows.Controls;

namespace SecureChat.Client.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();

        MainWindow.Instance.Title.Title = "SecureChat - Register";
    }

    private void Register_Click(object sender, System.Windows.RoutedEventArgs e)
    {

    }

    private void SignIn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(new LoginView());
    }
}
