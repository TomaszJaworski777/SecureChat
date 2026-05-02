using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}

[Authorize]
public class EventHub : Hub {
    private readonly DatabaseContext _context;

    public EventHub(DatabaseContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var nameIdentifier = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
        if (nameIdentifier is null)
            return;

        var userId = int.Parse(nameIdentifier);
        OnlineUsers.Add(userId);
        await Clients.Others.SendAsync("UserOnlineState", userId, true);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var nameIdentifier = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
        if (nameIdentifier is null)
            return;

        var userId = int.Parse(nameIdentifier);
        OnlineUsers.Remove(userId);
        await Clients.Others.SendAsync("UserOnlineState", userId, false);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(int receiverId, string content) {
        var user = Context.User;
        if (user is null)
            return;

        var nameIdentifier = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (nameIdentifier is null)
            return;

        if (!int.TryParse(nameIdentifier, out var senderId))
            return;

        Console.WriteLine("Message valid");

        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = EncryptionService.AES_Encrypt(content),
            SendDate = DateTime.UtcNow,
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        Console.WriteLine("Message saved");

        var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == senderId);
        var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == receiverId);
        if (sender is null || receiver is null)
            return;

        var payload = new
        {
            id = message.Id,
            senderId,
            senderUsername = EncryptionService.AES_Decrypt(sender.Username),
            recieverUsername = EncryptionService.AES_Decrypt(receiver.Username),
            content,
            date = message.SendDate,
        };

        await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", payload);

        Console.WriteLine("Message sent");
    }
}