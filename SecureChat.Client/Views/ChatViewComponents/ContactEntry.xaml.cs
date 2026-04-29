using System.Windows.Controls;
using System.Windows.Media;

namespace SecureChat.Client.Views.ChatViewComponents;

public partial class ContactEntry : UserControl
{
    private ChatView _chatView;
    private int _contactId; //TODO: Change id to full user class

    public ContactEntry(ChatView chatView, int id, string username, bool isOnline, string lastMessage, string lastActivityDate)
    {
        InitializeComponent();

        _chatView = chatView;
        _contactId = id;

        UsernameText.Text = username;
        InitialText.Text = username.Length > 0 ? username.First().ToString().ToUpper() : "?";
        LastMessageText.Text = lastMessage;
        LastMessageDateText.Text = lastActivityDate;
        OnlineIndicator.Visibility = isOnline ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        Avatar.Fill = new SolidColorBrush(GetAvatarColor(username));
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
        _chatView.SetCurrentMessageView(_contactId);
    }
}
