using System.Windows.Controls;

namespace SecureChat.Client.Views;

public partial class ChatView : UserControl
{
    private MainWindow _mainWindow;

    public ChatView(MainWindow mainWindow)
    {
        InitializeComponent();

        _mainWindow = mainWindow;

        _mainWindow.TitleBar.Title = "SecureChat - Username";
    }
}
