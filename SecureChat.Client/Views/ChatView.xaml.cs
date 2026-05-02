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
    private string _username = "";

    public ChatView(MainWindow mainWindow, List<Contact> contacts)
    {
        InitializeComponent();

        _mainWindow = mainWindow;
        _mainWindow.TitleBar.Title = "SecureChat";

        _ = LoadUsername();
        _ = LoadContactList(contacts);

        _mainWindow.Client.RegisterUserOnlineCallback((contactId, state) =>
        {
            Dispatcher.Invoke(() =>
            {
                var contact = ContactsList.Children.OfType<ContactEntry>()
                    .FirstOrDefault(entry => entry.Contact.ID == contactId);
                if (contact is null)
                    return;

                contact.SetOnlineState(state);
                UpdateContactList();
            });
        });

        _mainWindow.Client.RegisterNewUserCreatedCallback((contact) =>
        {
            Dispatcher.Invoke(() =>
            {
                ContactsList.Children.Add(new ContactEntry(this, contact));
                UpdateContactList();
            });
        });

        _mainWindow.Client.RegisterReceiveMessageCallback((message) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (_currentContact is null)
                    return;

                var contactEntry = ContactsList.Children.OfType<ContactEntry>().FirstOrDefault(e => e.Contact.ID == message.SenderID);
                if (contactEntry is null)
                    return;

                var contact = contactEntry.Contact;
                if (contact is null)
                    return;

                contactEntry.UpdateLastMessage(DecryptMessage(contact, message.Content));

                if (message.SenderID != _currentContact.ID)
                    return;

                DisplayMessage(contact, message, false);

                UpdateContactList();
            });
        });

        _mainWindow.Client.RegisterForceDisconnectCallback(() =>
        {
            Dispatcher.Invoke(() =>
            {
                _mainWindow.Navigate(new LoginView(_mainWindow));
            });
        });

        if (contacts == null || contacts.Count == 0)
        {
            ConversationTargetText.Text = "";
            _currentContact = null;
            return;
        }

        contacts = contacts
                    .OrderByDescending(e => e.IsOnline)
                    .ThenByDescending(e => e.LastMessageDate)
                    .ToList();

        _currentContact = contacts.First();
        _ = LoadMessagesList(_currentContact);
    }

    private async Task LoadUsername()
    {
        _username = await _mainWindow.Client.GetUsernameAsync();
        _mainWindow.TitleBar.Title = "SecureChat - " + _username;
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

        if (_currentContact is null)
            return;

        Messages.Children.Add(new MessageEntry(_username, message, MainWindow.DateToString(DateTime.UtcNow), true));
        MessagesScroll.ScrollToBottom();

        var contactEntry = ContactsList.Children.OfType<ContactEntry>().FirstOrDefault(e => e.Contact.ID == _currentContact.ID);
        if (contactEntry is not null && contactEntry.Contact is not null) {
            contactEntry.UpdateLastMessage(message);
            UpdateContactList();
        }

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

        var sorted = c
                    .OrderByDescending(e => e.IsOnline)
                    .ThenByDescending(e => e.LastMessageDate)
                    .ToList();

        foreach (var contact in sorted)
        {
            if (contact.LastMessage != "Start of the new conversation")
                contact.LastMessage = DecryptMessage(contact, contact.LastMessage);
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
            DisplayMessage(contact, message, isOurs);
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

    private void DisplayMessage(Contact contact, Message message, bool isOurs)
    {
        var content = DecryptMessage(contact, message.Content);
        Messages.Children.Add(new MessageEntry(message.SenderUsername, content, MainWindow.DateToString(message.Date), isOurs));
        MessagesScroll.ScrollToBottom();
    }

    private void UpdateContactList()
    {
        var sorted = ContactsList.Children
                    .OfType<ContactEntry>()
                    .OrderByDescending(e => e.Contact.IsOnline)
                    .ThenByDescending(e => e.Contact.LastMessageDate)
                    .ToList();

        ContactsList.Children.Clear();

        foreach (var entry in sorted) {
            ContactsList.Children.Add(entry);
        }
    }

    private string DecryptMessage(Contact contact, string message)
    {
        var privateEcdh = ECDiffieHellman.Create();
        privateEcdh.ImportPkcs8PrivateKey(Convert.FromBase64String(_mainWindow.Client.PrivateKey), out _);

        var publicEcdh = ECDiffieHellman.Create();
        publicEcdh.ImportSubjectPublicKeyInfo(Convert.FromBase64String(contact.PublicKey), out _);

        var sharedSecret = privateEcdh.DeriveKeyMaterial(publicEcdh.PublicKey);

        using var aes = new AesGcm(sharedSecret, AesGcm.TagByteSizes.MaxSize);
        var data = Convert.FromBase64String(message);
        var iv = data[..12];
        var tag = data[12..28];
        var encryptedMsg = data[28..];
        var plainMsg = new byte[encryptedMsg.Length];

        aes.Decrypt(iv, encryptedMsg, tag, plainMsg);

        return Encoding.UTF8.GetString(plainMsg);
    }
}
