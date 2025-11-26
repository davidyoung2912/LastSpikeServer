using GameplaySessionTracker.Services;
using GameplaySessionTracker.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Add CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=LastSpike;Integrated Security=true;TrustServerCertificate=true;";

// Register repositories as Singletons with connection string
builder.Services.AddSingleton<GameplaySessionTracker.Repositories.ISessionRepository>(
    sp => new GameplaySessionTracker.Repositories.SessionRepository(connectionString));
builder.Services.AddSingleton<GameplaySessionTracker.Repositories.IPlayerRepository>(
    sp => new GameplaySessionTracker.Repositories.PlayerRepository(connectionString));
builder.Services.AddSingleton<GameplaySessionTracker.Repositories.IGameBoardRepository>(
    sp => new GameplaySessionTracker.Repositories.GameBoardRepository(connectionString));

// Register services as Singletons
builder.Services.AddSingleton<ISessionService, SessionService>();
builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IGameBoardService, GameBoardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();
