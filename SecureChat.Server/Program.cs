using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;

Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var connectionString = $"Host={builder.Configuration["DB_HOST"]};" +
                       $"Database={builder.Configuration["DB_NAME"]};" +
                       $"Username={builder.Configuration["DB_USER"]};" +
                       $"Password={builder.Configuration["DB_PASSWORD"]};";

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(EncryptionService.JWT_Key),
        ValidateIssuer = false,
        ValidateAudience = false,
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context => {
            var token = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token))
                context.Token = token;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    while (true) {
        try {
            var context = services.GetRequiredService<DatabaseContext>();
            context.Database.Migrate();
            Console.WriteLine("Successfully applied migrations.");
            break;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occurred while applying migrations: {e.Message}. Waiting 5 seconds...");
            Thread.Sleep(5000);
        }
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<EventHub>("/events");

app.MapLoginEndpoints();
app.MapRegisterEndpoints();
app.MapUserEndpoints();
app.MapMessageEndpoints();

app.Run();