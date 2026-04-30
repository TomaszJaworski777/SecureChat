using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public static class UserEndpoints
{
    private class Contact
    {
        public int id { get; set; }
        public string username { get; set; } = "";
        public string lastMessage { get; set; } = "";
        public DateTime lastMessageDate { get; set; }
        public bool isOnline { get; set; } = false;
    }

    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/username", async (ClaimsPrincipal claims, DatabaseContext context) =>
        {
            var idString = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idString is null)
                return Results.Unauthorized();

            var userId = int.Parse(idString);
            var user = await context.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();

            return Results.Ok(new { username = EncryptionService.AES_Decrypt(user!.Username) });
        }).RequireAuthorization()
        .WithName("GetUsername");

        app.MapGet("/contacts", async (ClaimsPrincipal claims, DatabaseContext context) =>
        {
            var idString = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idString is null)
                return Results.Unauthorized();

            var userId = int.Parse(idString);
            var users = await context.Users.Where(x => x.Id != userId).ToListAsync();
            var contacts = new List<Contact>();

            var lastMessages = await context.Messages
                .Where(x => x.SenderId == userId || x.ReceiverId == userId)
                .GroupBy(x => x.SenderId == userId ? x.ReceiverId : x.SenderId)
                .Select(g => g.OrderByDescending(m => m.SendDate).First())
                .ToListAsync();

            foreach (var user in users)
            {
                var lastMessage = lastMessages.FirstOrDefault(m => m.SenderId == user.Id || m.ReceiverId == user.Id);
                var lastMessageContent = lastMessage is null ? "Start of the new conversation" : EncryptionService.AES_Decrypt(lastMessage.Content);
                var lastMessageDate = lastMessage is null ? user.CreationDate : lastMessage.SendDate;

                contacts.Add(new Contact
                {
                    id = user.Id,
                    username = EncryptionService.AES_Decrypt(user.Username),
                    lastMessage = lastMessageContent,
                    lastMessageDate = lastMessageDate,
                    isOnline = false,
                });
            }

            contacts.Sort((a, b) => a.lastMessageDate.CompareTo(b.lastMessageDate));

            return Results.Ok(contacts);
        }).RequireAuthorization()
        .WithName("GetContacts");
    }
}