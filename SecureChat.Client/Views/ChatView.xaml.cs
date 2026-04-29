using System.Windows.Controls;
using SecureChat.Client.Views.ChatViewComponents;

namespace SecureChat.Client.Views;

public partial class ChatView : UserControl
{
    private MainWindow _mainWindow;
    private int _contactId;

    public ChatView(MainWindow mainWindow)
    {
        InitializeComponent();

        _mainWindow = mainWindow;
        _mainWindow.TitleBar.Title = "SecureChat - Username";

        AddPlaceholderContacts();
    }

    private void AddPlaceholderContacts()
    {
        var contacts = new[]
        {
            ("Alice", 0,     true,  "Sure, see you then!",       "12:45 PM"),
            ("Bob", 1,        true,  "Can you send me the file? Can you send me the file? Can you send me the file? Can you send me the file?", "11:30 AM"),
            ("Charlie", 2,   false, "Thanks!",                   "Yesterday"),
            ("Diana", 3,     false, "lol okay",                  "Monday"),
            ("Eve", 4,       false, "Let me know",               "Apr 21"),
            ("Anna", 5,     true,  "Sure, see you then!",       "12:45 PM"),
            ("Boromir",  6,      true,  "Can you send me the file?", "11:30 AM"),
            ("Coca-Cola", 7, false, "Thanks!", "Yesterday"),
            ("Demon", 8, false, "lol okay", "Monday"),
            ("Evelynn", 9, false, "Let me know", "Apr 21"),
        };

        foreach (var (username, id, isOnline, lastMessage, date) in contacts)
        {
            ContactsList.Children.Add(new ContactEntry(this, id, username, isOnline, lastMessage, date));
        }

        var random = new Random();
        foreach (var (username, _, _, lastMessage, date) in contacts)
        {
            Messages.Children.Add(new MessageEntry(username, lastMessage, date, random.NextDouble() > 0.5));
        }

        MessagesScroll.UpdateLayout();
        MessagesScroll.ScrollToBottom();
    }

    public void SetCurrentMessageView(int id) {
        if (_contactId == id)
            return;

        _contactId = id;
    }

    private void SendButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var message = MessageBox.Text;

        MessageBox.Text = "";
    }

    private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
            SendButton_Click(sender, e);
    }
}
