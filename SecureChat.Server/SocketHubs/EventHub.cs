using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

[Authorize]
public class EventHub : Hub {
    //store online users on static list 
    //send singal for new contact on register
    //handle login/logout
    //send signar for new message

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

    public Task SendMessage(int receiverId, string content) {
        Console.WriteLine($"[{receiverId}]: {content}");
        return Task.CompletedTask;
    }
}