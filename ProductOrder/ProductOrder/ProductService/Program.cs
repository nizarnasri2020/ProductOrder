using Microsoft.EntityFrameworkCore;
using ProductService.Messaging;
using ProductService.Persistence;
using Serilog;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "ProductServiceLog-.txt");
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File(
        logFilePath,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();
// Add services to the container.
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("ProductDb")));

builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
