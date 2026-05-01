using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public static class MessageEndpoints
{
    private class Message
    {
        public int id { get; set; }
        public int senderId { get; set; }
        public string senderUsername { get; set; } = "";
        public string recieverUsername { get; set; } = "";
        public string content { get; set; } = "";
        public DateTime date { get; set; }
    }

    public static void MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/messages");

        group.MapGet("/", async (ClaimsPrincipal claims, DatabaseContext context) =>
        {
            var idString = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idString is null)
                return Results.Unauthorized();

            var userId = int.Parse(idString);
            var messages = await context.Messages.Where(x => x.SenderId == userId || x.ReceiverId == userId).OrderBy(x => x.SendDate).ToListAsync();
            var result = new List<Message>();

            foreach (var message in messages)
            {
                var senderUsername = await context.Users.Where(x => x.Id == message.SenderId).Select(x => x.Username).FirstOrDefaultAsync();
                var recieverUsername = await context.Users.Where(x => x.Id == message.ReceiverId).Select(x => x.Username).FirstOrDefaultAsync();

                result.Add(new Message
                {
                    id = message.Id,
                    senderId = message.SenderId,
                    senderUsername = EncryptionService.AES_Decrypt(senderUsername!),
                    recieverUsername = EncryptionService.AES_Decrypt(recieverUsername!),
                    content = EncryptionService.AES_Decrypt(message.Content),
                    date = message.SendDate
                });
            }

            return Results.Ok(result);
        }).RequireAuthorization()
        .WithName("GetAllMessages");

        group.MapGet("/{id}", async (string id, ClaimsPrincipal claims, DatabaseContext context) =>
        {
            var idString = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idString is null)
                return Results.Unauthorized();

            if (!int.TryParse(id, out var targetId))
                return Results.BadRequest();

            var userId = int.Parse(idString);
            var messages = await context.Messages.Where(x => 
                                (x.SenderId == userId && x.ReceiverId == targetId) || (x.SenderId == targetId && x.ReceiverId == userId))
                                .OrderBy(x => x.SendDate).ToListAsync();
            var result = new List<Message>();

            foreach (var message in messages)
            {
                var senderUsername = await context.Users.Where(x => x.Id == message.SenderId).Select(x => x.Username).FirstOrDefaultAsync();
                var recieverUsername = await context.Users.Where(x => x.Id == message.ReceiverId).Select(x => x.Username).FirstOrDefaultAsync();

                result.Add(new Message
                {
                    id = message.Id,
                    senderId = message.SenderId,
                    senderUsername = EncryptionService.AES_Decrypt(senderUsername!),
                    recieverUsername = EncryptionService.AES_Decrypt(recieverUsername!),
                    content = EncryptionService.AES_Decrypt(message.Content),
                    date = message.SendDate
                });
            }

            return Results.Ok(result);
        }).RequireAuthorization()
        .WithName("GetMessagesWithUser");
    }
}