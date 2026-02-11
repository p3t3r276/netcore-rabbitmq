using TestRabbit.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMqProducer>>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    return RabbitMqProducer.CreateAsync(logger, configuration).GetAwaiter().GetResult();
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
