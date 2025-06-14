using AlgoTradingAlpaca.Configurations;
using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Services;
using AlgoTradingAlpaca.Trading;
using Microsoft.EntityFrameworkCore;

namespace AlgoTradingAlpaca;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddUserSecrets<Program>();
        
        var connectionString = builder.Configuration.GetConnectionString("AlpacaTradingContextConnection") ?? throw
            new InvalidOperationException("Connection string not found");

        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        
        // Add services to the container.
        builder.Services.Configure<AlpacaConfig>(builder.Configuration.GetSection("Alpaca"));
        builder.Services.AddHttpClient<ITradingClientService, TradingClientService>();
        builder.Services.AddScoped<IPositionDataService, PositionDataService>();
        builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
        builder.Services.AddScoped<IBarDataService, BarDataService>();
        builder.Services.AddScoped<ITradingStrategy, TradingStrategy>();
        builder.Services.AddScoped<IAccountDataService, AccountDataService>();
        builder.Services.AddSingleton<ITradingBotService, TradingBotService>();
        
       
        builder.Services.AddSingleton<ITradingBotService, TradingBotService>();
        builder.Services.AddHostedService<TradingBotService>(provider => 
            (TradingBotService)provider.GetRequiredService<ITradingBotService>());

        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactLocalhost", policy =>
            {
                policy
                    .WithOrigins("http://localhost:3000") // React dev server
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowReactLocalhost");
        
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}