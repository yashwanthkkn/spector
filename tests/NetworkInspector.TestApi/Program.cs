using NetworkInspector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

// Add Network Inspector - this is the ONLY line needed!
builder.Services.AddNetworkInspector();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

// Use Network Inspector Middleware
app.UseNetworkInspector();

app.MapControllers();

app.Run();
