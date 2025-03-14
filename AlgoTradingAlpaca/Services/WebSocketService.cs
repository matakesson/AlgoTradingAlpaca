using System.Net.Security;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AlgoTradingAlpaca.Configurations;
using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using Microsoft.Extensions.Options;

namespace AlgoTradingAlpaca.Services;

public class WebSocketService : IWebSocketService
{
    private readonly AlpacaConfig _config;
    private ClientWebSocket _barsWebSocket;
    
    private IServiceProvider _serviceProvider;

    public WebSocketService(IOptions<AlpacaConfig> configOptions, IServiceProvider serviceProvider)
    {
        _config = configOptions.Value ?? throw new ArgumentNullException(nameof(configOptions));
        _serviceProvider = serviceProvider;
    }

    public async Task StartBarsWebSocketAsync(string[] symbols)
    {
        if (_barsWebSocket is { State: WebSocketState.Open })
        {
            Console.WriteLine("Bars Websocket is already running");
        }

        await ConnectToBarsWebSocketAsync();
        await SubScribeToBarsWebSocketAsync(symbols);
        _ = Task.Run(() => ReceiveMessageAsync(_barsWebSocket));
    }

    private async Task ConnectToBarsWebSocketAsync()
    {
        _barsWebSocket = new ClientWebSocket();
        await _barsWebSocket.ConnectAsync(new Uri(_config.WebSocketBarsUrl), CancellationToken.None);
        await AuthenticateAsync(_barsWebSocket);
    }
    
    private async Task AuthenticateAsync(ClientWebSocket clientWebSocket)
    {
        string authMessage = $"{{\"action\": \"auth\", \"key\": \"{_config.ApiKey}\", \"secret\": \"{_config.ApiSecret}\"}}";
        await SendMessageAsync(authMessage, clientWebSocket);
    }

    private async Task SubScribeToBarsWebSocketAsync(string[] barSymbols)
    {
        string symbolsList = string.Join("\",\"", barSymbols);
        string barsSubscriptíonMessage = $"{{ \"action\":\"subscribe\", \"bars\": [\"{symbolsList}\"] }}";
        Console.WriteLine($"Bars subscription message {barsSubscriptíonMessage}");
        
        await SendMessageAsync(barsSubscriptíonMessage, _barsWebSocket);
        Console.WriteLine($"Subscribed to bars: {string.Join(", ", barSymbols)}");
    }

    private async Task SendMessageAsync(string message, ClientWebSocket clientWebSocket)
    {
        if (clientWebSocket?.State != WebSocketState.Open)
        {
            Console.WriteLine("WebSocket is not open. Cannot send message");
        }
        else
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private async Task ReceiveMessageAsync(ClientWebSocket clientWebSocket)
    {
        byte[] buffer = new byte[4096];
    
        try
        {
            Console.WriteLine("Started listening for messages...");
    
            while (clientWebSocket?.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.EndOfMessage)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received message: {message}");
                    await ProcessMessageAsync(message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving messages: {ex.Message}");
        }
    }
    
    private async Task ProcessMessageAsync(string message)
    {
        try
        {
            var jsonDocument = JsonDocument.Parse(message);
            var root = jsonDocument.RootElement;
    
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    await ProcessSingleElementAsync(element);
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                await ProcessSingleElementAsync(root);
            }
            else
            {
                Console.WriteLine("Unexpected message type");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
        }
    }
    
    private async Task ProcessSingleElementAsync(JsonElement element)
    {
        if (element.TryGetProperty("T", out var typeElement))
        {
            string messageType = typeElement.GetString();

            if (messageType == "b") // For bars
            {
                if (element.TryGetProperty("S", out var symbolElement) &&
                    element.TryGetProperty("o", out var openElement) &&
                    element.TryGetProperty("h", out var highElement) &&
                    element.TryGetProperty("l", out var lowElement) &&
                    element.TryGetProperty("c", out var closeElement) &&
                    element.TryGetProperty("t", out var timeElement) &&
                    element.TryGetProperty("v", out var volumeElement))
                {
                    var barData = new BarData()
                    {
                        Symbol = symbolElement.GetString(),
                        OpenPrice = openElement.GetDouble(),
                        HighPrice = highElement.GetDouble(),
                        LowPrice = lowElement.GetDouble(),
                        ClosingPrice = closeElement.GetDouble(),
                        TimeStamp = DateTime.Parse(timeElement.GetString()),
                        Volume = volumeElement.GetDouble()
                    };

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        dbContext.Add(barData);
                        await dbContext.SaveChangesAsync();
                    }
                    
                    Console.WriteLine($"Processed bar: {barData.Symbol} - Close: {barData.ClosingPrice}");
                } 
            }
        }
    }

}