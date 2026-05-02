using System.Windows.Controls;
using System.Windows.Media;
using static SecureChat.Client.API.ApiClient;

namespace SecureChat.Client.Views.ChatViewComponents;

public partial class ContactEntry : UserControl
{
    public Contact Contact { get; private set; }

    private ChatView _chatView;

    public ContactEntry(ChatView chatView, Contact contact)
    {
        InitializeComponent();

        _chatView = chatView;
        Contact = contact;

        UsernameText.Text = Contact.Username;
        InitialText.Text = Contact.Username.Length > 0 ? Contact.Username.First().ToString().ToUpper() : "?";
        LastMessageText.Text = Contact.LastMessage;
        LastMessageDateText.Text = MainWindow.DateToString(Contact.LastMessageDate);
        Avatar.Fill = new SolidColorBrush(GetAvatarColor(Contact.Username));

        SetOnlineState(contact.IsOnline);
    }

    public void SetOnlineState(bool state) {
        OnlineIndicator.Visibility = state ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        Contact.IsOnline = state;
    }

    public void UpdateLastMessage(string message) {
        Contact.LastMessage = message;
        Contact.LastMessageDate = DateTime.UtcNow;
        LastMessageText.Text = message;
        LastMessageDateText.Text = MainWindow.DateToString(Contact.LastMessageDate);
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
        _chatView.SetCurrentMessageView(Contact);
    }
}
