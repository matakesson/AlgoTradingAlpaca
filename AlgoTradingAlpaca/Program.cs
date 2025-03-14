using AlgoTradingAlpaca.Configurations;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Services;

namespace AlgoTradingAlpaca;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddUserSecrets<Program>();
        
        // Add services to the container.
        builder.Services.Configure<AlpacaConfig>(builder.Configuration.GetSection("Alpaca"));
        builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

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
    }
}