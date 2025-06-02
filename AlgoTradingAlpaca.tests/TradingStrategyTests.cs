using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Trading;
using AlgoTradingAlpaca.Trading.Strategies;

namespace AlgoTradingAlpaca.tests;

public class TradingStrategyTests
{
    private readonly ICalculateBullishReversal _calculateBullishReversal;
    private readonly ICalculateBearishReversal _calculateBearishReversal;
    
    public TradingStrategyTests()
    {
        _calculateBullishReversal = new CalculateBullishReversal();
        _calculateBearishReversal = new CalculateBearishReversal();
    }

    [Fact]
    public async Task CalculatebullishReversalSignal_ReturnValidSignal_ConditionMet()
    {
        // Arrange
        var barData = Mock.BullishReversalBarsMockSetup.GenerateValidBullishSignalData("AAPL");

        // Act
        var signal = await _calculateBullishReversal.CalculateBullishReversalSignal(barData);
        
        // Assert
        Assert.NotNull(signal);
        Assert.True(signal!.IsBreakout);
        Assert.True(signal.EntryPrice > 0);
        Assert.True(signal.Point1 > 0);
        Assert.True(signal.Point2 > signal.Point1);
    }

    [Fact]
    public async Task CalculateBearishReversalSignal_ReturnValidSignal_ConditionsMet()
    {
        // Arrange
        var barData = Mock.BearishReversalBarsMockSetup.GenerateValidBearishSignalData("AAPL");
        
        // Act
        var signal = await _calculateBearishReversal.CalculateBearishReversalSignal(barData);
        
        // Assert
        Assert.NotNull(signal);
        Assert.True(signal!.IsBreakout);
        Assert.True(signal.EntryPrice > 0);
        Assert.True(signal.Point1 > 0);
        Assert.True(signal.Point2 < signal.Point1);
    }
    
    [Fact]
    public async void BullishReversal_NotEnoughData_ReturnsNull()
    {
        // Arrange
        var data = Mock.BullishReversalBarsMockSetup.GenerateValidBullishSignalData("AAPL").Take(10).ToList();

        // Act
        var signal = await _calculateBullishReversal.CalculateBullishReversalSignal(data);

        // Assert 
        Assert.Null(signal);
    }
    
    [Fact]
    public async void BullishReversal_NoDowntrend_ReturnsNull()
    {
        // Arrange 
        var data = Mock.BullishReversalBarsMockSetup.GenerateFlatMarketData("AAPL");

        // Act
        var signal = await _calculateBullishReversal.CalculateBullishReversalSignal(data);

        // Assert
        Assert.Null(signal);
    }
    
    [Fact]
    public async void BullishReversal_NoBullishTrend_ReturnsNull()
    {
        // Arrange 
        var data = Mock.BullishReversalBarsMockSetup.GenerateOnlyDowntrendData("APPL");

        // Act
        var signal = await _calculateBullishReversal.CalculateBullishReversalSignal(data);

        // Assert
        Assert.Null(signal);
    }
    
    [Fact]
    public async void BullishReversal_ShortBearishTrend_ReturnsNull()
    {
        // Arrange 
        var data = Mock.BullishReversalBarsMockSetup.GenerateShortBearishTrendData("AAPL");

        // Act
        var signal = await _calculateBullishReversal.CalculateBullishReversalSignal(data);

        // Assert
        Assert.Null(signal);
    }

    [Fact]
    public async void BullishReversal_NoBreakout_ReturnsNull()
    {
        // Arrange 
        var data = Mock.BullishReversalBarsMockSetup.GenerateNoBreakoutData("AAPL");

        // Act
        var signal = await _calculateBullishReversal.CalculateBullishReversalSignal(data);

        // Assert
        Assert.Null(signal);
    }

    [Fact]
    public async void BullishReversal_StrongBreakout_ReturnsCorrectLevels()
    {
        // Arrange
        var data = Mock.BullishReversalBarsMockSetup.GenerateStrongBreakoutData("AAPL");
        double expectedLowPoint = 85.0;
        double expectedBullishHigh = 96.0;
        double expectedBreakoutPrice = 100.0;

        // Act
        var signal = await _calculateBullishReversal.CalculateBullishReversalSignal(data);

        // Assert
        Assert.NotNull(signal);
        Assert.Equal(expectedLowPoint, signal!.Point1, 0);
        Assert.Equal(expectedBullishHigh, signal.Point2, 0);
        Assert.Equal(expectedBreakoutPrice, signal.EntryPrice, 0);
    }

    [Fact]
    public async void BullishReversal_UnorderedBars_StillFindsSignal()
    {
        // Arrange 
        var data = Mock.BullishReversalBarsMockSetup.GenerateValidBullishSignalData("AAPL");
        var shuffled = data.OrderBy(x => Guid.NewGuid()).ToList();  

        // Act
        var signal = await _calculateBullishReversal.CalculateBullishReversalSignal(shuffled);

        // Assert
        Assert.NotNull(signal);
        Assert.True(signal!.IsBreakout);
    }
}