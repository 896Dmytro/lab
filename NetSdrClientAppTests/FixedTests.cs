using Xunit;
using NetSdrClientApp.Messages;
using NetSdrClientApp;

namespace NetSdrClientAppTests;

public class FixedMessageTests
{
    [Fact]
    public void CreateConnectMessage_ValidTarget_ShouldCreateMessage()
    {
        // Arrange
        var targetName = "TEST";
        
        // Act
        var message = NetSdrMessageHelper.CreateConnectMessage(targetName);
        
        // Assert
        Assert.NotNull(message);
        Assert.Equal(MessageType.Command, message.Header.MessageType);
    }
    
    [Fact]
    public void TryParse_InvalidData_ShouldReturnFalse()
    {
        // Arrange
        var invalidData = new byte[] { 0x00, 0x01 };
        
        // Act
        var result = NetSdrMessage.TryParse(invalidData, out var message);
        
        // Assert
        Assert.False(result);
        Assert.Null(message);
    }
    
    [Fact]
    public void CreateDataMessage_WithPayload_ShouldSetCorrectLength()
    {
        // Arrange
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        
        // Act
        var message = NetSdrMessageHelper.CreateDataMessage
