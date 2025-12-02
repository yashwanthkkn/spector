using NetworkInspector;
using Spector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

// Add Network Inspector - this is the ONLY line needed!
builder.Services.AddNetworkInspector();
builder.Services.AddSpector();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSpector();
app.UseAuthorization();

// Use Network Inspector Middleware
//app.UseNetworkInspector();


app.MapControllers();

app.Run();
