using System.Windows.Controls;
using Wpf.Ui.Controls;
using SecureChat.Client.Views;
using SecureChat.Client.API;

namespace SecureChat.Client;

public partial class MainWindow : FluentWindow
{
    public ApiClient Client { get; private set; }

    public MainWindow()
    {
        InitializeComponent();

        Client = new ApiClient();
        Navigate(new LoginView(this));
    }

    public void Navigate(UserControl view)
    {
        MainContent.Content = view;
    }
}