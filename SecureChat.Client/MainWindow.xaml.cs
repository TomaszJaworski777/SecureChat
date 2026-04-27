using System.Windows.Controls;
using Wpf.Ui.Controls;
using SecureChat.Client.Views;

namespace SecureChat.Client;

public partial class MainWindow : FluentWindow
{
    public static MainWindow Instance { get; private set; } = null!;

    public MainWindow()
    {
        Instance = this;
        InitializeComponent();

        Navigate(new LoginView());
    }

    public void Navigate(UserControl view)
    {
        MainContent.Content = view;
    }
}