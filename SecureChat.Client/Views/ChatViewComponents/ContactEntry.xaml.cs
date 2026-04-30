using System.Windows.Controls;
using System.Windows.Media;
using static SecureChat.Client.API.ApiClient;

namespace SecureChat.Client.Views.ChatViewComponents;

public partial class ContactEntry : UserControl
{
    private ChatView _chatView;
    private Contact _contact;

    public ContactEntry(ChatView chatView, Contact contact)
    {
        InitializeComponent();

        _chatView = chatView;
        _contact = contact;

        UsernameText.Text = _contact.Username;
        InitialText.Text = _contact.Username.Length > 0 ? _contact.Username.First().ToString().ToUpper() : "?";
        LastMessageText.Text = _contact.LastMessage;
        LastMessageDateText.Text = MainWindow.DateToString(_contact.LastMessageDate);
        OnlineIndicator.Visibility = _contact.IsOnline ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        Avatar.Fill = new SolidColorBrush(GetAvatarColor(_contact.Username));
    }

    private static Color GetAvatarColor(string username)
    {
        var hash = 0;
        foreach (var c in username)
            hash = (hash * 31 + c) & 0xFFFF;

        double hue = (hash % 360) / 360.0;
        double saturation = 0.5;
        double value = 0.7;

        return HsvToRgb(hue, saturation, value);
    }

    private static Color HsvToRgb(double h, double s, double v)
    {
        int i = (int)(h * 6);
        double f = h * 6 - i;
        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);

        double r, g, b;
        switch (i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }

        return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _chatView.SetCurrentMessageView(_contact);
    }
}
