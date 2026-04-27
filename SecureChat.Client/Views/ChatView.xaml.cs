using System.Windows.Controls;

namespace SecureChat.Client.Views;

public partial class ChatView : UserControl
{
    public ChatView()
    {
        InitializeComponent();

        MainWindow.Instance.Title.Title = "SecureChat - Username";
    }
}
