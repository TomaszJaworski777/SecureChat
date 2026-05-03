using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

[Authorize]
public class StateHub : Hub {
    private readonly DatabaseContext _context;
    private List<int> _onlineUserId;

    public StateHub(DatabaseContext context)
    {
        _context = context;

        _onlineUserId = [];
    }

    public bool IsOnline(int id) => _onlineUserId.Contains(id);


}