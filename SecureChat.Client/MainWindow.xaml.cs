using System.Windows.Controls;
using Wpf.Ui.Controls;
using SecureChat.Client.Views;
using SecureChat.Client.API;

namespace SecureChat.Client;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        var auth = new Auth();
        Navigate(new LoginView(this, auth));
    }

    public void Navigate(UserControl view)
    {
        MainContent.Content = view;
    }
}