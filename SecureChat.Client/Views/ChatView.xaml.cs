using System.Windows.Controls;
using SecureChat.Client.Views.ChatViewComponents;
using static SecureChat.Client.API.ApiClient;

namespace SecureChat.Client.Views;

public partial class ChatView : UserControl
{
    private MainWindow _mainWindow;
    private int _currentMessageId;

    public ChatView(MainWindow mainWindow, List<Contact> contacts)
    {
        InitializeComponent();

        _mainWindow = mainWindow;
        _mainWindow.TitleBar.Title = "SecureChat";

        _ = SetProperUsername();
        _ = LoadContactList(contacts);

        if (contacts == null || contacts.Count == 0) {
            ConversationTargetText.Text = "";
            _currentMessageId = -1;
            return;
        }

        _ = LoadMessagesList(contacts.First());
    }

    private async Task SetProperUsername()
    {
        var username = await _mainWindow.Client.GetUsernameAsync();
        _mainWindow.TitleBar.Title = "SecureChat - " + username;
    }

    public void SetCurrentMessageView(Contact contact)
    {
        if (_currentMessageId == contact.ID)
            return;

        _currentMessageId = contact.ID;

        _ = LoadMessagesList(contact);
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

    private async Task LoadContactList(List<Contact>? contacts = null)
    {
        var c = contacts ?? await _mainWindow.Client.GetContactsAsync();
        ContactsList.Children.Clear();

        foreach (var contact in c)
        {
            ContactsList.Children.Add(new ContactEntry(this, contact));
        }
    }

    private async Task LoadMessagesList(Contact contact)
    {
        var messages = await _mainWindow.Client.GetMessagesAsync(contact.ID);
        Messages.Children.Clear();

        foreach (var message in messages)
        {
            var isOurs = message.SenderID != contact.ID;
            Messages.Children.Add(new MessageEntry(message.SenderUsername, message.Content, MainWindow.DateToString(message.Date), isOurs));
        }

        ConversationTargetText.Text = contact.Username;

        MessagesScroll.ScrollToBottom();
    }
}
