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

    public static string DateToString(DateTime utcDate)
    {
        var local = utcDate.ToLocalTime();
        var today = DateTime.Now.Date;

        if (local.Date == today)
            return local.ToString("h:mm tt");

        if (local.Date == today.AddDays(-1))
            return "Yesterday, " + local.ToString("h:mm tt");

        return local.ToString("M/d/yyyy h:mm tt");
    }
}