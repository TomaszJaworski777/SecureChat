using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using SecureChat.Client.Views.ChatViewComponents;
using static SecureChat.Client.API.ApiClient;

namespace SecureChat.Client.Views;

public partial class ChatView : UserControl
{
    private MainWindow _mainWindow;
    private Contact? _currentContact;

    public ChatView(MainWindow mainWindow, List<Contact> contacts)
    {
        InitializeComponent();

        _mainWindow = mainWindow;
        _mainWindow.TitleBar.Title = "SecureChat";

        _ = SetProperUsername();
        _ = LoadContactList(contacts);

        _mainWindow.Client.RegisterUserOnlineCallback((contactId, state) =>
        {
            Dispatcher.Invoke(() =>
            {
                var contact = ContactsList.Children.OfType<ContactEntry>()
                    .FirstOrDefault(entry => entry.ContactID == contactId);
                if (contact is null)
                    return;

                contact.SetOnlineState(state);
            });
        });

        _mainWindow.Client.RegisterNewUserCreatedCallback((contact) =>
        {
            Dispatcher.Invoke(() =>
            {
                ContactsList.Children.Add(new ContactEntry(this, contact));
            });
        });

        _mainWindow.Client.RegisterMessageReceivedCallback((message) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (_currentContact is null)
                    return;

                if (message.SenderID != _currentContact.ID)
                    return;

                Messages.Children.Add(new MessageEntry(message.SenderUsername, message.Content, MainWindow.DateToString(message.Date), false));
            });
        });

        _mainWindow.Client.RegisterForceDisconnectCallback(() =>
        {
            Dispatcher.Invoke(() => {
                _mainWindow.Navigate(new LoginView(_mainWindow));
            });
        });

        if (contacts == null || contacts.Count == 0)
        {
            ConversationTargetText.Text = "";
            _currentContact = null;
            return;
        }

        _currentContact = contacts.First();
        _ = LoadMessagesList(_currentContact);
    }

    private async Task SetProperUsername()
    {
        var username = await _mainWindow.Client.GetUsernameAsync();
        _mainWindow.TitleBar.Title = "SecureChat - " + username;
    }

    public void SetCurrentMessageView(Contact contact)
    {
        if (_currentContact?.ID == contact.ID)
            return;

        _currentContact = contact;

        _ = LoadMessagesList(contact);
    }

    private void SendButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var message = MessageBox.Text;

        MessageBox.Text = "";

        if (_currentContact is not null)
            _ = _mainWindow.Client.SendMessageAsync(_currentContact, message);
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

        if (!string.IsNullOrWhiteSpace(contact.PublicKey))
        {
            var a = Convert.FromBase64String(_mainWindow.Client.PublicKey);
            var b = Convert.FromBase64String(contact.PublicKey);
            byte[] mergedKey = _mainWindow.Client.PublicKey.CompareTo(contact.PublicKey) < 0 ? [.. a, .. b] : [.. b, .. a];
            var fingerPrint = Convert.ToHexString(SHA256.HashData(mergedKey));

            FingerprintText.Text = fingerPrint[..32];
        }

        ConversationTargetText.Text = contact.Username;

        MessagesScroll.ScrollToBottom();
    }
}
